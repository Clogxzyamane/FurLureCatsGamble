using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TurnState { Start, PlayerTurn, EnemyTurn, Victory, Defeat }

public class TurnBasedSystem : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;

    public Transform playerPoint;
    public Transform enemyPoint1;
    public Transform enemyPoint2;
    public Transform enemyPoint3;

    // runtime instance references (cached so we can clean them up)
    GameObject playerGO;
    GameObject enemyGO1;
    GameObject enemyGO2;
    GameObject enemyGO3;

    PlayerUnit playerUnit;
    PlayerUnit enemyUnit1;
    PlayerUnit enemyUnit2;
    PlayerUnit enemyUnit3;

    // Arrays for easier iteration/management of enemy spots and HUDs
    PlayerUnit[] enemyUnits;
    TurnBasedHUD[] enemyHUDs;

    public TextMeshProUGUI dialogue;

    public TurnBasedHUD playerHUD;
    public TurnBasedHUD enemy1HUD;
    public TurnBasedHUD enemy2HUD;
    public TurnBasedHUD enemy3HUD;

    public TurnState State;

    // Prevent overlapping turn coroutines
    bool isProcessingTurn = false;

    // Dodge configuration (player can press Space during enemy attack window)
    public float dodgeWindowSeconds = 1.5f; // how long player has to press Space
    [Range(0f, 1f)]
    public float dodgeSuccessChance = 0.75f; // probability that a space press results in successful dodge

    // Card / deck fields
    public List<Card> deck = new List<Card>(); // place cards here in inspector or populate at runtime
    public Card[] hand = new Card[3]; // three card slots
    public Button[] cardButtons; // assign 3 buttons in inspector, optional
    public TextMeshProUGUI[] cardLabels; // assign 3 labels in inspector, optional

    // FreeShot (Focus / bullets) fields
    [Header("FreeShot (Focus)")]
    [Range(0, 9)]
    public int maxFocusPoints = 9;
    public int focusPoints = 9; // current bullets / focus
    public Button freeShotButton; // button to toggle free-shot mode (assign in inspector)
    public TextMeshProUGUI focusLabel; // UI label to show current focus/bullets

    // internal selection state
    int selectedHandIndex = -1;
    bool cardPlayedThisTurn = false;

    // free-shot runtime state
    bool isInFreeShotMode = false;
    public int freeShotDamage = 1; // damage per free shot (can use playerUnit.damage instead)

    void Start()        
    {
        State = TurnState.Start;
        StartCoroutine(SetupFight());
    }

    IEnumerator SetupFight()
    {
        // Instantiate player and enemies at the spawn transforms (use position+rotation so they appear correctly)
        playerGO = Instantiate(player, playerPoint.position, playerPoint.rotation);
        playerGO.SetActive(true);
        playerUnit = playerGO.GetComponent<PlayerUnit>();
        if (playerUnit != null)
        {
            // Ensure runtime fields are initialized so HUD shows correct values immediately
            playerUnit.currentHP = playerUnit.maxHP;
            playerUnit.damage = playerUnit.baseDamage;
        }

        enemyGO1 = Instantiate(enemy1, enemyPoint1.position, enemyPoint1.rotation);
        enemyGO1.SetActive(true);
        enemyUnit1 = enemyGO1.GetComponent<PlayerUnit>();
        if (enemyUnit1 != null)
        {
            enemyUnit1.currentHP = enemyUnit1.maxHP;
            enemyUnit1.damage = enemyUnit1.baseDamage;
        }

        enemyGO2 = Instantiate(enemy2, enemyPoint2.position, enemyPoint2.rotation);
        enemyGO2.SetActive(true);
        enemyUnit2 = enemyGO2.GetComponent<PlayerUnit>();
        if (enemyUnit2 != null)
        {
            enemyUnit2.currentHP = enemyUnit2.maxHP;
            enemyUnit2.damage = enemyUnit2.baseDamage;
        }

        enemyGO3 = Instantiate(enemy3, enemyPoint3.position, enemyPoint3.rotation);
        enemyGO3.SetActive(true);
        enemyUnit3 = enemyGO3.GetComponent<PlayerUnit>();
        if (enemyUnit3 != null)
        {
            enemyUnit3.currentHP = enemyUnit3.maxHP;
            enemyUnit3.damage = enemyUnit3.baseDamage;
        }

        // Build arrays for iteration
        enemyUnits = new PlayerUnit[] { enemyUnit1, enemyUnit2, enemyUnit3 };
        enemyHUDs = new TurnBasedHUD[] { enemy1HUD, enemy2HUD, enemy3HUD };

        // Initialize HUDs (null-checks to avoid runtime errors)
        if (playerUnit != null && playerHUD != null)
        {
            playerHUD.SetHUD(playerUnit);
            // show bullets immediately on player HUD
            playerHUD.SetBullets(focusPoints, maxFocusPoints);
            // status effects will be set by caller mapping; clear for now
            playerHUD.SetStatusEffects(null);
        }

        for (int i = 0; i < enemyUnits.Length; i++)
        {
            if (enemyUnits[i] != null && enemyHUDs[i] != null)
            {
                enemyHUDs[i].SetHUD(enemyUnits[i]);
                enemyHUDs[i].SetStatusEffects(null); // set status sprites later via your registry
            }
        }

        // Prepare card UI listeners (safe null-checks)
        if (cardButtons != null)
        {
            for (int i = 0; i < cardButtons.Length; i++)
            {
                int idx = i;
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => OnPlayCard(idx));
            }
        }

        // Wire freeShot button safely
        if (freeShotButton != null)
        {
            freeShotButton.onClick.RemoveAllListeners();
            freeShotButton.onClick.AddListener(OnToggleFreeShot);
        }

        // Ensure focusPoints is within range and update UI
        focusPoints = Mathf.Clamp(focusPoints, 0, maxFocusPoints);
        UpdateCardUI();

        // Shuffle deck and draw initial hand
        ShuffleDeck();
        for (int i = 0; i < hand.Length; i++)
            DrawToHandSlot(i);

        UpdateCardUI();

        // Yield one frame to ensure all objects/HUDs are initialized and visible
        yield return null;

        State = TurnState.PlayerTurn;
        // Start the PlayerTurn coroutine properly
        StartCoroutine(PlayerTurn());
    }

    void Update()
    {
        // Handle free-shot targeting when active
        if (isInFreeShotMode && State == TurnState.PlayerTurn && focusPoints > 0)
        {
            // Prevent clicks over UI from firing shots
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Camera cam = Camera.main;
                if (cam == null) return;

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100f))
                {
                    // attempt to find a PlayerUnit on the hit object or its parents
                    PlayerUnit hitUnit = hit.collider.GetComponentInParent<PlayerUnit>();
                    if (hitUnit != null && hitUnit != playerUnit)
                    {
                        AttemptFreeShot(hitUnit);
                    }
                }
            }
        }
    }

    IEnumerator PlayerTurn()
    {
        if (isProcessingTurn) yield break;
        if (State != TurnState.PlayerTurn) yield break;
        isProcessingTurn = true;

        dialogue.text = "Play your cards";
        UpdateCardUI();

        // Wait until the player plays one card (or a timeout could be added)
        selectedHandIndex = -1;
        cardPlayedThisTurn = false;
        while (!cardPlayedThisTurn)
            yield return null;

        // turn will continue after card play
        int slot = selectedHandIndex;
        if (slot < 0 || slot >= hand.Length || hand[slot] == null)
        {
            // invalid selection - end turn gracefully
            dialogue.text = "Invalid card selection.";
            yield return new WaitForSeconds(0.8f);
            State = TurnState.EnemyTurn;
            StartCoroutine(EnemyTurn());
            isProcessingTurn = false;
            yield break;
        }

        // Target first alive enemy
        PlayerUnit target = GetFirstAliveEnemy();
        if (target == null)
        {
            State = TurnState.Victory;
            EndFight();
            isProcessingTurn = false;
            yield break;
        }

        // Apply card damage
        Card played = hand[slot];
        float dmgMult = 1f;
        var playerLevel = playerGO != null ? playerGO.GetComponent<PlayerLevel>() : null;
        if (playerLevel != null) dmgMult = playerLevel.GetDamageMultiplier();
        int finalDamage = Mathf.RoundToInt(played.damage * dmgMult);
        bool isDead = target.TakeDamage(finalDamage);

        // Update enemy HUD
        UpdateEnemyHUDForTarget(target);

        dialogue.text = "You play " + played.cardName + " and hit " + target.unitName + " for " + played.damage + " damage";
        yield return new WaitForSeconds(1.2f);

        // Remove played card and refill slot
        hand[slot] = null;
        DrawToHandSlot(slot);
        UpdateCardUI();

        if (isDead)
        {
            DestroyEnemyGOForTarget(target);

            if (AreAllEnemiesDead())
            {
                State = TurnState.Victory;
                EndFight();
            }
            else
            {
                State = TurnState.EnemyTurn;
                StartCoroutine(EnemyTurn());
            }
        }
        else
        {
            State = TurnState.EnemyTurn;
            StartCoroutine(EnemyTurn());
        }

        // reset selection state
        selectedHandIndex = -1;
        cardPlayedThisTurn = false;
        isProcessingTurn = false;
    }

    // Attempt a free-shot at the provided target; deducts one focus point
    void AttemptFreeShot(PlayerUnit target)
    {
        if (target == null) return;
        if (State != TurnState.PlayerTurn) return;
        if (focusPoints <= 0) return;

        focusPoints = Mathf.Max(0, focusPoints - 1);
        UpdateCardUI();

        // Use player's damage or dedicated freeShotDamage
        int damageToApply = (playerUnit != null ? playerUnit.damage : freeShotDamage);
        bool isDead = target.TakeDamage(damageToApply);

        UpdateEnemyHUDForTarget(target);
        dialogue.text = "Freeshot! You hit " + target.unitName + " for " + damageToApply + " damage";

        if (isDead)
        {
            DestroyEnemyGOForTarget(target);
            if (AreAllEnemiesDead())
            {
                State = TurnState.Victory;
                EndFight();
                return;
            }
        }

        // Auto-exit free-shot mode when no focus points remain
        if (focusPoints <= 0)
            ExitFreeShotMode();
    }

    // Toggle free-shot mode via UI
    public void OnToggleFreeShot()
    {
        if (State != TurnState.PlayerTurn) return;
        if (focusPoints <= 0)
        {
            dialogue.text = "No bullets remaining!";
            return;
        }

        isInFreeShotMode = !isInFreeShotMode;
        dialogue.text = isInFreeShotMode ? "FreeShot enabled: click an enemy to shoot." : "FreeShot disabled.";
        UpdateCardUI();
    }

    void ExitFreeShotMode()
    {
        isInFreeShotMode = false;
        UpdateCardUI();
    }

    IEnumerator EnemyTurn()
    {
        if (isProcessingTurn) yield break;
        if (State != TurnState.EnemyTurn) yield break;
        isProcessingTurn = true;

        // Each alive enemy gets to act once in enemy phase
        foreach (var enemy in enemyUnits)
        {
            if (enemy == null) continue;
            if (enemy.currentHP <= 0) continue;

            dialogue.text = enemy.unitName + " attacks!";
            yield return new WaitForSeconds(0.6f);

            // Safety: ensure player exists
            if (playerUnit == null)
            {
                Debug.LogWarning("PlayerUnit missing during EnemyTurn.");
                State = TurnState.Defeat;
                EndFight();
                isProcessingTurn = false;
                yield break;
            }

            // Give the player a short window to press Space to attempt a dodge
            bool dodged = false;
            float timer = 0f;
            dialogue.text = enemy.unitName + " is preparing to strike — press SPACE to dodge!";
            while (timer < dodgeWindowSeconds)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // Determine dodge success by configured chance
                    float baseChance = dodgeSuccessChance;
                    var playerLevel = playerGO != null ? playerGO.GetComponent<PlayerLevel>() : null;
                    float bonus = (playerLevel != null) ? playerLevel.GetDodgeChanceBonus() : 0f;
                    float successChance = Mathf.Clamp01(baseChance + bonus);
                    if (Random.value <= successChance)
                    {
                        dodged = true;
                    }
                    else
                    {
                        dodged = false;
                    }
                    break;
                }
                timer += Time.deltaTime;
                yield return null; // wait one frame
            }

            if (dodged)
            {
                dialogue.text = "You dodged " + enemy.unitName + "'s attack!";
                // Slight pause so player sees the result
                yield return new WaitForSeconds(0.9f);
                // Update player HUD (no HP change)
                if (playerHUD != null)
                    playerHUD.SetHP(playerUnit.currentHP);
                // Continue to next enemy
                continue;
            }

            // No successful dodge: apply damage
            bool isDead = playerUnit.TakeDamage(enemy.damage);

            // Update player HUD
            if (playerHUD != null)
                playerHUD.SetHP(playerUnit.currentHP);

            dialogue.text = enemy.unitName + " attacks you for " + enemy.damage + " damage";
            yield return new WaitForSeconds(1.1f);

            if (isDead)
            {
                State = TurnState.Defeat;
                EndFight();
                isProcessingTurn = false;
                yield break;
            }
        }

        // After all enemies acted, return to player turn if player still alive
        if (playerUnit != null && playerUnit.currentHP > 0)
        {
            State = TurnState.PlayerTurn;
            StartCoroutine(PlayerTurn());
        }

        isProcessingTurn = false;
    }

    void EndFight()
    {
        // Stop any running turns and prevent further actions
        StopAllCoroutines();
        isProcessingTurn = false;

        if (State == TurnState.Victory)
        {
            dialogue.text = "You won!";
        }
        else if (State == TurnState.Defeat)
        {
            dialogue.text = "You Lost!";
        }

        // Cleanup instantiated objects (optional - keeps scene tidy)
        if (playerGO != null) { Destroy(playerGO); playerGO = null; playerUnit = null; }
        DestroyIfExists(enemyGO1); enemyGO1 = null; enemyUnit1 = null;
        DestroyIfExists(enemyGO2); enemyGO2 = null; enemyUnit2 = null;
        DestroyIfExists(enemyGO3); enemyGO3 = null; enemyUnit3 = null;

        // Optionally, clear HUDs or disable UI controls here
    }

    public void OnAttackButton()
    {
        if (State != TurnState.PlayerTurn)
            return;

        if (!isProcessingTurn)
            StartCoroutine(PlayerTurn());
    }

    // Called by UI buttons (assign in inspector or wired up in SetupFight)
    public void OnPlayCard(int handIndex)
    {
        if (State != TurnState.PlayerTurn) return;
        if (isProcessingTurn == false) return; // ensure we're in PlayerTurn coroutine waiting for selection
        if (handIndex < 0 || handIndex >= hand.Length) return;
        if (hand[handIndex] == null) return;

        selectedHandIndex = handIndex;
        cardPlayedThisTurn = true;
    }

    // Helper: returns the first enemy unit with HP > 0
    PlayerUnit GetFirstAliveEnemy()
    {
        if (enemyUnits == null) return null;
        foreach (var e in enemyUnits)
        {
            if (e != null && e.currentHP > 0)
                return e;
        }
        return null;
    }

    // Helper: update the HUD corresponding to the targeted enemy
    void UpdateEnemyHUDForTarget(PlayerUnit target)
    {
        if (enemyUnits == null || enemyHUDs == null) return;
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            if (enemyUnits[i] == target && enemyHUDs[i] != null)
            {
                enemyHUDs[i].SetHP(target.currentHP);
                break;
            }
        }
    }

    // Helper: destroy enemy GameObject corresponding to a dead target (keeps arrays intact)
    void DestroyEnemyGOForTarget(PlayerUnit target)
    {
        if (target == enemyUnit1) DestroyIfExists(enemyGO1);
        else if (target == enemyUnit2) DestroyIfExists(enemyGO2);
        else if (target == enemyUnit3) DestroyIfExists(enemyGO3);
    }

    void DestroyIfExists(GameObject go)
    {
        if (go != null)
            Destroy(go);
    }

    // Helper: returns true if all enemies are dead or null
    bool AreAllEnemiesDead()
    {
        if (enemyUnits == null) return true;
        foreach (var e in enemyUnits)
        {
            if (e != null && e.currentHP > 0)
                return false;
        }
        return true;
    }

    // Draws the top card from deck into specified hand slot (or clears if deck empty)
    void DrawToHandSlot(int slot)
    {
        if (slot < 0 || slot >= hand.Length) return;
        if (deck != null && deck.Count > 0)
        {
            // draw top (index 0)
            hand[slot] = deck[0];
            deck.RemoveAt(0);
        }
        else
        {
            hand[slot] = null;
        }
    }

    // Update the UI for cards (safe to call even if UI references are null)
    void UpdateCardUI()
    {
        if (cardLabels != null)
        {
            for (int i = 0; i < cardLabels.Length; i++)
            {
                if (i < hand.Length && hand[i] != null)
                    cardLabels[i].text = hand[i].cardName + " (" + hand[i].damage + ")";
                else
                    cardLabels[i].text = "-";
            }
        }

        if (cardButtons != null)
        {
            for (int i = 0; i < cardButtons.Length; i++)
            {
                bool interactable = (i < hand.Length && hand[i] != null && State == TurnState.PlayerTurn);
                cardButtons[i].interactable = interactable;
            }
        }

        if (freeShotButton != null)
        {
            // free-shot is available while it's the player's turn and they have bullets
            freeShotButton.interactable = (State == TurnState.PlayerTurn && focusPoints > 0);
            // optional visual: change button text or color depending on isInFreeShotMode
        }

        if (focusLabel != null)
            focusLabel.text = "Bullets: " + focusPoints + " / " + maxFocusPoints;
    }

    // Simple Fisher-Yates shuffle for the deck
    void ShuffleDeck()
    {
        if (deck == null || deck.Count <= 1) return;
        for (int i = 0; i < deck.Count - 1; i++)
        {
            int j = Random.Range(i, deck.Count);
            var tmp = deck[i];
            deck[i] = deck[j];
            deck[j] = tmp;
        }
    }
}


