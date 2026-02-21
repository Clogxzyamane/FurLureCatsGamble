// ─────────────────────────────────────────────────────────────────────────────
//  TurnBasedSystem.cs  —  Final
//
//  Dependencies (all must be in your project):
//    • PlayerUnit.cs     — player character component
//    • EnemyData.cs      — EnemyUnit, EnemyAttack, StatusEffect, all enums
//    • TurnBasedHUD.cs   — HUD for both player and enemies
//    • Card.cs           — your existing Card ScriptableObject / class
//    • PlayerLevel.cs    — your existing PlayerLevel component (optional)
//
//  Turn order:  PlayerTurn → Enemy1 → Enemy2 → Enemy3 → repeat
//  Free-shot:   E key toggles; clicking enemy button spends 1 bullet.
//  Dodge:       SPACE during dodge window; success = no damage + 1 bullet.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TurnState { Start, PlayerTurn, EnemyTurn, Victory, Defeat }

public class TurnBasedSystem : MonoBehaviour
{
    // ── Prefabs ───────────────────────────────────────────────────────────────

    [Header("Unit Prefabs")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject enemy1Prefab;
    [SerializeField] GameObject enemy2Prefab;
    [SerializeField] GameObject enemy3Prefab;

    [Header("Spawn Points")]
    [SerializeField] Transform playerPoint;
    [SerializeField] Transform enemyPoint1;
    [SerializeField] Transform enemyPoint2;
    [SerializeField] Transform enemyPoint3;

    // ── Runtime unit references ───────────────────────────────────────────────

    GameObject playerGO;
    PlayerUnit playerUnit;                          // Echo — uses PlayerUnit

    readonly GameObject[] enemyGOs   = new GameObject[3];
    readonly EnemyUnit[]  enemyUnits = new EnemyUnit[3];  // enemies use EnemyUnit

    // ── UI ────────────────────────────────────────────────────────────────────

    [Header("Dialogue")]
    [SerializeField] TextMeshProUGUI dialogue;

    [Header("HUDs")]
    [SerializeField] TurnBasedHUD playerHUD;
    [SerializeField] TurnBasedHUD enemy1HUD;
    [SerializeField] TurnBasedHUD enemy2HUD;
    [SerializeField] TurnBasedHUD enemy3HUD;
    TurnBasedHUD[] enemyHUDs;

    [Header("Enemy Target Buttons")]
    [SerializeField] Button enemyButton1;
    [SerializeField] Button enemyButton2;
    [SerializeField] Button enemyButton3;
    Button[] enemyButtons;

    [Header("Card Buttons")]
    [SerializeField] Button cardButton1;
    [SerializeField] Button cardButton2;
    [SerializeField] Button cardButton3;

    [Header("Free-Shot")]
    [SerializeField] Button          freeShotButton;
    [SerializeField] TextMeshProUGUI focusLabel;
    [SerializeField] TextMeshProUGUI freeShotStateText;

    [Header("Dodge HUD")]
    [SerializeField] TextMeshProUGUI dodgeText;

    // ── Cards ─────────────────────────────────────────────────────────────────

    [Header("Card System")]
    [Tooltip("Drag CardBase assets here to populate the deck. " +
             "Create cards via: Right-click Project → Cards → Card Base.")]
    [SerializeField] List<CardBase> deck = new List<CardBase>();
    CardBase[]     activeCardSlots = new CardBase[3];
    List<CardBase> discardPile     = new List<CardBase>();

    // ── Bullets / Focus ───────────────────────────────────────────────────────

    [Header("Focus (Bullets)")]
    [Range(0, 9)] [SerializeField] int maxFocusPoints = 9;
    [SerializeField]               int focusPoints    = 9;
  


    // ── Dodge timing ──────────────────────────────────────────────────────────

    [Header("Dodge Timing")]
    [SerializeField] float attackTime       = 1.5f;
    [SerializeField] float dodgeWindowStart = 0.5f;
    [SerializeField] float dodgeWindowEnd = 0.9f;

    // ── bullets On Dodge  ──────────────────────────────────────────────────────────

    [Header("bullets On Dodge")]
    [SerializeField] int bulletsOnDodge = 1;

    // ── State ─────────────────────────────────────────────────────────────────

    [Header("Debug — read only")]
    [SerializeField] TurnState currentState;

    bool isProcessingTurn     = false;
    int  selectedCardSlot     = -1;
    bool cardConfirmed        = false;
    int  selectedEnemyIndex   = -1;
    bool enemyConfirmed       = false;
    bool isInFreeShotMode     = false;
    bool dodgePressedThisFrame = false;

    // ─────────────────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        currentState = TurnState.Start;
        StartCoroutine(SetupFight());
    }

    void Update()
    {
        dodgePressedThisFrame = Input.GetKeyDown(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.E) && currentState == TurnState.PlayerTurn)
            ToggleFreeShotMode();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K) && enemyUnits[0] != null)
            StartCoroutine(EnemyAttackSequence(enemyUnits[0], enemyUnits[0].damage));
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  SETUP
    // ─────────────────────────────────────────────────────────────────────────

    IEnumerator SetupFight()
    {
        // ── Spawn player ──────────────────────────────────────────────────────
        playerGO   = SpawnUnit(playerPrefab, playerPoint);
        playerUnit = playerGO != null ? playerGO.GetComponent<PlayerUnit>() : null;
        if (playerUnit != null)
        {
            playerUnit.currentHP = playerUnit.maxHP;
            playerUnit.damage    = playerUnit.baseDamage;
        }

        // ── Spawn enemies ─────────────────────────────────────────────────────
        Transform[]  spawnPts = { enemyPoint1, enemyPoint2, enemyPoint3 };
        GameObject[] prefabs  = { enemy1Prefab, enemy2Prefab, enemy3Prefab };

        for (int i = 0; i < 3; i++)
        {
            enemyGOs[i]   = SpawnUnit(prefabs[i], spawnPts[i]);
            enemyUnits[i] = enemyGOs[i] != null ? enemyGOs[i].GetComponent<EnemyUnit>() : null;
            if (enemyUnits[i] != null)
            {
                enemyUnits[i].currentHP = enemyUnits[i].maxHP;
                enemyUnits[i].damage    = enemyUnits[i].baseDamage;
            }
        }

        // ── Build HUD array ───────────────────────────────────────────────────
        enemyHUDs = new TurnBasedHUD[] { enemy1HUD, enemy2HUD, enemy3HUD };

        // ── Initialise player HUD ─────────────────────────────────────────────
        if (playerHUD != null && playerUnit != null)
        {
            playerHUD.SetPlayerHUD(playerUnit, focusPoints, maxFocusPoints);
        }

        // ── Initialise enemy HUDs ─────────────────────────────────────────────
        for (int i = 0; i < 3; i++)
        {
            if (enemyHUDs[i] != null && enemyUnits[i] != null)
                enemyHUDs[i].SetEnemyHUD(enemyUnits[i]);
        }

        // ── Build enemy button array & wire listeners ─────────────────────────
        enemyButtons = new Button[] { enemyButton1, enemyButton2, enemyButton3 };
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            if (enemyButtons[i] != null)
            {
                enemyButtons[i].onClick.RemoveAllListeners();
                enemyButtons[i].onClick.AddListener(() => OnEnemyButtonClicked(idx));
            }
        }

        // ── Wire card buttons ─────────────────────────────────────────────────
        Button[] cardBtns = { cardButton1, cardButton2, cardButton3 };
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            if (cardBtns[i] != null)
            {
                cardBtns[i].onClick.RemoveAllListeners();
                cardBtns[i].onClick.AddListener(() => OnCardButtonClicked(idx));
            }
        }

        // ── Wire free-shot button ─────────────────────────────────────────────
        if (freeShotButton != null)
        {
            freeShotButton.onClick.RemoveAllListeners();
            freeShotButton.onClick.AddListener(ToggleFreeShotMode);
        }

        // ── Deck ──────────────────────────────────────────────────────────────
        focusPoints = Mathf.Clamp(focusPoints, 0, maxFocusPoints);
        ShuffleDeck();
        for (int i = 0; i < activeCardSlots.Length; i++)
            DrawToSlot(i);

        RefreshUI();
        yield return null;

        currentState = TurnState.PlayerTurn;
        StartCoroutine(PlayerTurn());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PLAYER TURN
    // ─────────────────────────────────────────────────────────────────────────

    IEnumerator PlayerTurn()
    {
        if (isProcessingTurn || currentState != TurnState.PlayerTurn) yield break;
        isProcessingTurn = true;

        // Tick player DoT effects (fire/poison applied by enemies)
        if (playerUnit != null)
        {
            int playerDot = playerUnit.TickStatusEffects();
            if (playerDot > 0)
            {
                playerHUD?.SetHP(playerUnit.currentHP);
                playerHUD?.SetStatusEffectsFromUnit(playerUnit.activeEffects);
                dialogue.text = $"You take {playerDot} from status effects!";
                yield return new WaitForSeconds(0.6f);

                if (playerUnit.currentHP <= 0)
                {
                    currentState = TurnState.Defeat;
                    EndFight();
                    isProcessingTurn = false;
                    yield break;
                }
            }
        }

        dialogue.text = "Your turn — pick a card  (E = Free Shot)";
        ExitFreeShotMode();
        RefreshUI();

        // Wait for card selection (free-shot may happen freely in the meantime)
        selectedCardSlot = -1;
        cardConfirmed    = false;

        while (!cardConfirmed)
        {
            if (isInFreeShotMode)
                dialogue.text = "FREE SHOT — click an enemy  (E = back to cards)";
            yield return null;
        }

        // ── Card was selected — ask for target ────────────────────────────────
        int      cardSlot = selectedCardSlot;
        CardBase played   = activeCardSlots[cardSlot];

        if (played == null)
        {
            dialogue.text = "That slot is empty.";
            yield return new WaitForSeconds(0.6f);
            isProcessingTurn = false;
            StartCoroutine(PlayerTurn());
            yield break;
        }

        dialogue.text = "Choose a target";
        selectedEnemyIndex = -1;
        enemyConfirmed     = false;
        SetEnemyButtonsInteractable(true);

        while (!enemyConfirmed)
            yield return null;

        SetEnemyButtonsInteractable(false);

        int targetIdx = selectedEnemyIndex;
        if (targetIdx < 0 || targetIdx >= 3 ||
            enemyUnits[targetIdx] == null ||
            enemyUnits[targetIdx].currentHP <= 0)
        {
            dialogue.text = "Invalid target.";
            yield return new WaitForSeconds(0.6f);
            isProcessingTurn = false;
            StartCoroutine(PlayerTurn());
            yield break;
        }

        // ── Resolve card ──────────────────────────────────────────────────────
        EnemyUnit target = enemyUnits[targetIdx];
        var       lvl    = playerGO != null ? playerGO.GetComponent<PlayerLevel>() : null;
        float     mult   = lvl != null ? lvl.GetDamageMultiplier() : 1f;
        int       dmg    = Mathf.RoundToInt(played.cardDamage * mult);
        bool      isDead = target.TakeDamage(dmg);

        UpdateEnemyHUD(targetIdx);
        dialogue.text = $"You play {played.cardName} → {target.unitName} for {dmg} dmg!";

        // Apply any status effects the card carries
        foreach (var effectType in played.statusEffects)
        {
            if (effectType == StatusEffectType.None) continue;
            target.ApplyStatusEffect(new StatusEffect(
                effectType,
                played.effectDuration,
                played.effectDotDamage,
                played.effectIcon));
        }
        UpdateEnemyHUD(targetIdx); // refresh again after effects applied

        discardPile.Add(played);
        activeCardSlots[cardSlot] = null;
        DrawToSlot(cardSlot);

        yield return new WaitForSeconds(1.2f);
        if (isDead) KillEnemy(targetIdx);
        RefreshUI();

        if (AreAllEnemiesDead())
        {
            currentState = TurnState.Victory;
            EndFight();
            isProcessingTurn = false;
            yield break;
        }

        isProcessingTurn = false;
        currentState = TurnState.EnemyTurn;
        StartCoroutine(EnemyTurnSequence());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ENEMY TURN SEQUENCE  (Enemy1 → Enemy2 → Enemy3)
    // ─────────────────────────────────────────────────────────────────────────

    IEnumerator EnemyTurnSequence()
    {
        if (isProcessingTurn || currentState != TurnState.EnemyTurn) yield break;
        isProcessingTurn = true;

        SetCardUIVisible(false);

        for (int i = 0; i < 3; i++)
        {
            EnemyUnit enemy = enemyUnits[i];
            if (enemy == null || enemy.currentHP <= 0) continue;

            // Tick enemy DoT (player applied fire/poison to this enemy)
            int dot = enemy.TickStatusEffects();
            if (dot > 0)
            {
                UpdateEnemyHUD(i);
                dialogue.text = $"{enemy.unitName} takes {dot} from effects!";
                yield return new WaitForSeconds(0.5f);

                if (enemy.currentHP <= 0)
                {
                    KillEnemy(i);
                    if (AreAllEnemiesDead())
                    {
                        currentState = TurnState.Victory;
                        EndFight();
                        isProcessingTurn = false;
                        yield break;
                    }
                    continue;
                }
            }

            // ── AI picks attack or heal ────────────────────────────────────────
            int attackIdx = enemy.ChooseAttackIndex();

            enemyHUDs[i]?.HighlightAttack(attackIdx);
            enemyHUDs[i]?.RefreshAttacks(enemy);

            dialogue.text = $"{enemy.unitName}'s turn!";
            yield return new WaitForSeconds(0.5f);

            if (playerUnit == null)
            {
                currentState = TurnState.Defeat;
                EndFight();
                isProcessingTurn = false;
                yield break;
            }

            if (attackIdx == -1)
            {
                // Heal move
                int healed = enemy.Heal(enemy.healMove.healAmount);
                UpdateEnemyHUD(i);
                dialogue.text = $"{enemy.unitName} heals for {enemy.healMove.healAmount}!";
                yield return new WaitForSeconds(0.8f);
            }
            else
            {
                // Damage attack
                EnemyAttack chosen = (enemy.attacks != null && attackIdx < enemy.attacks.Count)
                    ? enemy.attacks[attackIdx]
                    : null;

                int attackDmg = chosen != null ? chosen.damage : enemy.damage;

                yield return StartCoroutine(EnemyAttackSequence(enemy, attackDmg));

                // Apply status effect to player if the attack has one
                if (chosen != null &&
                    chosen.appliedEffect != StatusEffectType.None &&
                    playerUnit != null)
                {
                    playerUnit.ApplyStatusEffect(
                        new StatusEffect(chosen.appliedEffect,
                                         chosen.effectDuration,
                                         chosen.effectDotDamage,
                                         chosen.effectIcon));
                    playerHUD?.SetStatusEffectsFromUnit(playerUnit.activeEffects);
                }
            }

            enemyHUDs[i]?.ClearAttackHighlights();

            if (currentState == TurnState.Defeat)
            {
                isProcessingTurn = false;
                yield break;
            }
        }

        // ── Return to player ──────────────────────────────────────────────────
        isProcessingTurn = false;

        if (playerUnit != null && playerUnit.currentHP > 0)
        {
            SetCardUIVisible(true);
            currentState = TurnState.PlayerTurn;
            StartCoroutine(PlayerTurn());
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ENEMY ATTACK SEQUENCE  (single enemy attacks; player may dodge)
    // ─────────────────────────────────────────────────────────────────────────

    public IEnumerator EnemyAttackSequence(EnemyUnit attacker, int damage)
    {
        if (attacker == null) yield break;

        float total    = Mathf.Max(0.01f, attackTime);
        float winStart = Mathf.Clamp01(dodgeWindowStart) * total;
        float winEnd   = Mathf.Clamp01(dodgeWindowEnd)   * total;
        float t        = 0f;
        bool  dodged   = false;

        dodgePressedThisFrame = false;

        while (t < total)
        {
            t += Time.deltaTime;
            float remaining = Mathf.Max(0f, total - t);

            if (t < winStart)
            {
                if (dodgeText != null) dodgeText.text = $"Incoming: {remaining:F1}s";
            }
            else if (t <= winEnd)
            {
                if (dodgeText != null) dodgeText.text = "DODGE!  [ SPACE ]";
                if (dodgePressedThisFrame) { dodged = true; break; }
            }
            else
            {
                if (dodgeText != null) dodgeText.text = $"Too late: {remaining:F1}s";
            }

            dodgePressedThisFrame = false;
            yield return null;

            // Second check — catches input that arrived after yield
            if (dodgePressedThisFrame && t >= winStart && t <= winEnd)
            {
                dodged = true;
                break;
            }
        }

        if (dodgeText != null) dodgeText.text = "";

        if (dodged)
        {
            // Successful dodge → award +1 bullet
            focusPoints = Mathf.Min(focusPoints + bulletsOnDodge, maxFocusPoints);
            playerHUD?.SetBullets(focusPoints, maxFocusPoints);
            UpdateFocusLabel();
            dialogue.text = "Dodged! +1 bullet";
            yield return new WaitForSeconds(0.8f);
            yield break;
        }

        // Attack lands
        if (playerUnit != null)
        {
            bool playerDead = playerUnit.TakeDamage(damage);
            playerHUD?.SetHP(playerUnit.currentHP);
            dialogue.text = $"{attacker.unitName} hits you for {damage}!";

            if (playerDead)
            {
                currentState = TurnState.Defeat;
                EndFight();
            }
        }

        yield return new WaitForSeconds(0.8f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FREE SHOT
    // ─────────────────────────────────────────────────────────────────────────

    public void ToggleFreeShotMode()
    {
        if (currentState != TurnState.PlayerTurn) return;

        if (isInFreeShotMode)
        {
            ExitFreeShotMode();
            dialogue.text = "Free shot off — pick a card";
        }
        else
        {
            if (focusPoints <= 0)
            {
                dialogue.text = "No bullets left!";
                return;
            }
            isInFreeShotMode = true;
            if (freeShotStateText != null) freeShotStateText.text = "FreeShot: ON";
            dialogue.text = "FREE SHOT — click an enemy  (E = back to cards)";
        }

        RefreshUI();
    }

    void ExitFreeShotMode()
    {
        isInFreeShotMode = false;
        if (freeShotStateText != null) freeShotStateText.text = "FreeShot: OFF";
    }

    void PerformFreeShot(int enemyIndex)
    {
        if (focusPoints <= 0 || enemyIndex < 0 || enemyIndex >= 3) return;

        EnemyUnit target = enemyUnits[enemyIndex];
        if (target == null || target.currentHP <= 0) return;

        var   lvl  = playerGO != null ? playerGO.GetComponent<PlayerLevel>() : null;
        float mult = lvl != null ? lvl.GetDamageMultiplier() : 1f;
        int   dmg  = Mathf.RoundToInt((playerUnit != null ? playerUnit.damage : 1) * mult);
        bool  dead = target.TakeDamage(dmg);

        focusPoints = Mathf.Max(0, focusPoints - 1);
        playerHUD?.SetBullets(focusPoints, maxFocusPoints);
        UpdateFocusLabel();
        UpdateEnemyHUD(enemyIndex);

        dialogue.text = $"Bang! {target.unitName} hit for {dmg}  [{focusPoints} bullets left]";

        if (dead)
        {
            KillEnemy(enemyIndex);
            if (AreAllEnemiesDead())
            {
                currentState = TurnState.Victory;
                EndFight();
                return;
            }
        }

        if (focusPoints <= 0)
        {
            ExitFreeShotMode();
            dialogue.text = "Out of bullets — pick a card";
        }

        RefreshUI();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BUTTON CALLBACKS
    // ─────────────────────────────────────────────────────────────────────────

    public void OnCardButtonClicked(int slotIndex)
    {
        if (currentState != TurnState.PlayerTurn) return;
        if (isInFreeShotMode) return;
        if (!isProcessingTurn) return;
        if (slotIndex < 0 || slotIndex >= activeCardSlots.Length) return;
        if (activeCardSlots[slotIndex] == null) return;

        selectedCardSlot = slotIndex;
        cardConfirmed    = true;
    }

    public void OnEnemyButtonClicked(int enemyIndex)
    {
        if (currentState != TurnState.PlayerTurn) return;

        if (isInFreeShotMode)
        {
            PerformFreeShot(enemyIndex);
            return;
        }

        if (!isProcessingTurn) return;
        if (enemyIndex < 0 || enemyIndex >= 3) return;
        if (enemyUnits[enemyIndex] == null || enemyUnits[enemyIndex].currentHP <= 0) return;

        selectedEnemyIndex = enemyIndex;
        enemyConfirmed     = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  END FIGHT
    // ─────────────────────────────────────────────────────────────────────────

    void EndFight()
    {
        StopAllCoroutines();
        isProcessingTurn = false;

        dialogue.text = currentState == TurnState.Victory ? "Victory!" : "Defeated…";

        SetCardUIVisible(false);
        SetEnemyButtonsInteractable(false);

        if (playerGO != null) { Destroy(playerGO); playerGO = null; playerUnit = null; }
        for (int i = 0; i < 3; i++) KillEnemy(i);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    GameObject SpawnUnit(GameObject prefab, Transform point)
    {
        if (prefab == null || point == null) return null;
        var go = Instantiate(prefab, point.position, point.rotation);
        go.SetActive(true);
        return go;
    }

    void KillEnemy(int index)
    {
        if (index < 0 || index >= 3) return;
        if (enemyGOs[index]   != null) { Destroy(enemyGOs[index]); enemyGOs[index] = null; }
        enemyUnits[index] = null;
        if (enemyHUDs   != null && enemyHUDs[index]   != null) enemyHUDs[index].gameObject.SetActive(false);
        if (enemyButtons != null && enemyButtons[index] != null) enemyButtons[index].gameObject.SetActive(false);
    }

    void UpdateEnemyHUD(int index)
    {
        if (index < 0 || index >= 3) return;
        if (enemyHUDs[index] != null && enemyUnits[index] != null)
        {
            enemyHUDs[index].SetHP(enemyUnits[index].currentHP);
            enemyHUDs[index].SetStatusEffectsFromUnit(enemyUnits[index].activeEffects);
        }
    }

    bool AreAllEnemiesDead()
    {
        for (int i = 0; i < 3; i++)
            if (enemyUnits[i] != null && enemyUnits[i].currentHP > 0)
                return false;
        return true;
    }

    void DrawToSlot(int slot)
    {
        if (slot < 0 || slot >= activeCardSlots.Length) return;
        if (deck.Count == 0 && discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
        activeCardSlots[slot] = deck.Count > 0 ? deck[0] : null;
        if (deck.Count > 0) deck.RemoveAt(0);
    }

    void ShuffleDeck()
    {
        if (deck == null || deck.Count <= 1) return;
        for (int i = 0; i < deck.Count - 1; i++)
        {
            int      j   = Random.Range(i, deck.Count);
            CardBase tmp = deck[i]; deck[i] = deck[j]; deck[j] = tmp;
        }
    }

    void RefreshUI()
    {
        bool isPlayerTurn = currentState == TurnState.PlayerTurn;

        Button[] cardBtns = { cardButton1, cardButton2, cardButton3 };
        for (int i = 0; i < cardBtns.Length; i++)
        {
            if (cardBtns[i] == null) continue;
            CardBase slot         = i < activeCardSlots.Length ? activeCardSlots[i] : null;
            bool interactable = isPlayerTurn && !isInFreeShotMode && slot != null;
            cardBtns[i].interactable = interactable;
            var label = cardBtns[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = slot != null ? $"{slot.cardName}\n({slot.cardDamage} dmg)" : "—";
        }

        for (int i = 0; i < 3; i++)
        {
            if (enemyButtons == null || enemyButtons[i] == null) continue;
            bool alive = enemyUnits[i] != null && enemyUnits[i].currentHP > 0;
            enemyButtons[i].interactable = isPlayerTurn && alive && isInFreeShotMode;
        }

        if (freeShotButton != null)
            freeShotButton.interactable = isPlayerTurn && focusPoints > 0;

        UpdateFocusLabel();

        if (freeShotStateText != null)
            freeShotStateText.text = isInFreeShotMode ? "FreeShot: ON" : "FreeShot: OFF";
    }

    void SetCardUIVisible(bool visible)
    {
        foreach (var btn in new[] { cardButton1, cardButton2, cardButton3 })
            if (btn != null) btn.gameObject.SetActive(visible);
        if (freeShotButton != null) freeShotButton.gameObject.SetActive(visible);
    }

    void SetEnemyButtonsInteractable(bool interactable)
    {
        if (enemyButtons == null) return;
        for (int i = 0; i < 3; i++)
        {
            if (enemyButtons[i] == null) continue;
            bool alive = enemyUnits[i] != null && enemyUnits[i].currentHP > 0;
            enemyButtons[i].interactable = interactable && alive;
        }
    }

    void UpdateFocusLabel()
    {
        if (focusLabel != null)
            focusLabel.text = $"Bullets: {focusPoints} / {maxFocusPoints}";
    }
}
