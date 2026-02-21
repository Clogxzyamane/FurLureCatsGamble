// ─────────────────────────────────────────────────────────────────────────────
//  TurnBasedHUD.cs  —  Improved
//
//  Handles TWO modes set via IsPlayerHUD in the Inspector:
//
//  PLAYER HUD:
//    • Player name ("Echo")
//    • Health slider + text
//    • Up to 9 bullet icons (full / spent sprite swap)
//    • Status effect icons (effects the player has received)
//
//  ENEMY HUD:
//    • Enemy name
//    • Health slider + text
//    • Enemy Type  (LawLure / Bandit / Neutral)
//    • Affiliation (None / Law / Lure / Gang1-3 / Indigenous)
//    • Role        (Gunman / Sniper / Tank / Support / Archer / MiniBoss / Boss)
//    • Status effect icons (effects the player applied to this enemy)
//    • Attack panel: Attack1, Attack2, Attack3, Heal — each shows name, damage, effect icon
//
//  Call SetPlayerHUD(PlayerUnit, bullets, maxBullets) to initialise the player panel.
//  Call SetEnemyHUD(EnemyUnit) to initialise an enemy panel.
//  Incremental update methods (SetHP, SetBullets, SetStatusEffects, RefreshAttacks)
//  keep the display in sync as combat progresses.
//
//  Integration with TurnBasedSystem:
//    • TurnBasedSystem already calls SetHP, SetBullets, SetStatusEffects.
//    • After an enemy's attack AI runs, call RefreshAttacks(enemyUnit) to update
//      the attack display (e.g. to highlight the chosen move).
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TurnBasedHUD : MonoBehaviour
{
    // ── Mode ──────────────────────────────────────────────────────────────────

    [Header("HUD Mode")]
    [Tooltip("True = this HUD is for the player. False = enemy HUD.")]
    public bool isPlayerHUD = false;

    // ── Shared ────────────────────────────────────────────────────────────────

    [Header("─── Shared ───────────────────────")]
    [Tooltip("Displays the unit's name.")]
    public TextMeshProUGUI nameText;

    [Tooltip("Displays current HP as a number.")]
    public TextMeshProUGUI healthText;

    [Tooltip("Slider for HP. Set Min = 0; Max is set at runtime.")]
    public Slider healthSlider;

    [Header("Status Effects (icons applied to this unit)")]
    [Tooltip("Pool of Image slots, filled left-to-right with active effect icons.")]
    public Image[] statusEffectIcons;

    // ── Player-only ───────────────────────────────────────────────────────────

    [Header("─── Player Only ─────────────────────")]
    [Tooltip("Up to 9 Image slots for bullet icons.")]
    public Image[] bulletIcons;

    [Tooltip("Sprite used when a bullet is available (loaded).")]
    public Sprite bulletFullSprite;

    [Tooltip("Sprite used when a bullet has been spent.")]
    public Sprite bulletSpentSprite;

    // ── Enemy-only ────────────────────────────────────────────────────────────

    [Header("─── Enemy Only ──────────────────────")]
    [Tooltip("Displays EnemyType label.")]
    public TextMeshProUGUI enemyTypeText;

    [Tooltip("Displays Affiliation label.")]
    public TextMeshProUGUI affiliationText;

    [Tooltip("Displays Role label.")]
    public TextMeshProUGUI roleText;

    [Header("Attack Slots (Enemy)")]
    [Tooltip("Root GameObject for the attacks panel. Hidden on player HUD.")]
    public GameObject attacksPanel;

    // Each slot: name label, damage label, status-effect icon
    [Tooltip("Name label for Attack 1.")]
    public TextMeshProUGUI attack1Name;
    [Tooltip("Damage label for Attack 1.")]
    public TextMeshProUGUI attack1Damage;
    [Tooltip("Status effect icon for Attack 1 (optional).")]
    public Image attack1EffectIcon;

    [Tooltip("Name label for Attack 2.")]
    public TextMeshProUGUI attack2Name;
    [Tooltip("Damage label for Attack 2.")]
    public TextMeshProUGUI attack2Damage;
    [Tooltip("Status effect icon for Attack 2 (optional).")]
    public Image attack2EffectIcon;

    [Tooltip("Name label for Attack 3.")]
    public TextMeshProUGUI attack3Name;
    [Tooltip("Damage label for Attack 3.")]
    public TextMeshProUGUI attack3Damage;
    [Tooltip("Status effect icon for Attack 3 (optional).")]
    public Image attack3EffectIcon;

    [Tooltip("Name label for the Heal move.")]
    public TextMeshProUGUI healName;
    [Tooltip("Heal amount label.")]
    public TextMeshProUGUI healAmount;

    // Optional: highlight image shown on whichever attack the enemy just chose
    [Tooltip("Highlight overlay on Attack 1 slot (optional).")]
    public Image attack1Highlight;
    [Tooltip("Highlight overlay on Attack 2 slot (optional).")]
    public Image attack2Highlight;
    [Tooltip("Highlight overlay on Attack 3 slot (optional).")]
    public Image attack3Highlight;
    [Tooltip("Highlight overlay on Heal slot (optional).")]
    public Image healHighlight;

    // ─────────────────────────────────────────────────────────────────────────
    //  PUBLIC INIT  —  Player
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Full player HUD initialisation.
    /// Call once at fight start, then use incremental updates.
    /// </summary>
    public void SetPlayerHUD(PlayerUnit unit, int bullets, int maxBullets)
    {
        if (unit == null) return;

        if (nameText != null) nameText.text = unit.unitName; // should be "Echo"
        SetMaxHP(unit.maxHP);
        SetHP(unit.currentHP);
        SetBullets(bullets, maxBullets);
        SetStatusEffects(null);

        // Hide enemy-only elements
        if (attacksPanel     != null) attacksPanel.SetActive(false);
        if (enemyTypeText    != null) enemyTypeText.gameObject.SetActive(false);
        if (affiliationText  != null) affiliationText.gameObject.SetActive(false);
        if (roleText         != null) roleText.gameObject.SetActive(false);
    }

    /// <summary>Backward-compatible overload used by TurnBasedSystem.SetHUD(unit).</summary>
    public void SetHUD(PlayerUnit unit)
    {
        if (unit == null) return;
        if (nameText != null) nameText.text = unit.unitName;
        SetMaxHP(unit.maxHP);
        SetHP(unit.currentHP);
        SetStatusEffects(null);

        if (isPlayerHUD)
        {
            if (attacksPanel    != null) attacksPanel.SetActive(false);
            if (enemyTypeText   != null) enemyTypeText.gameObject.SetActive(false);
            if (affiliationText != null) affiliationText.gameObject.SetActive(false);
            if (roleText        != null) roleText.gameObject.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PUBLIC INIT  —  Enemy
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Full enemy HUD initialisation. Call once per fight start.
    /// </summary>
    public void SetEnemyHUD(EnemyUnit unit)
    {
        if (unit == null) return;

        if (nameText != null) nameText.text = unit.unitName;
        SetMaxHP(unit.maxHP);
        SetHP(unit.currentHP);
        SetStatusEffects(null);

        // Classification labels
        if (enemyTypeText != null)
        {
            enemyTypeText.text = unit.enemyType.ToString();
            enemyTypeText.gameObject.SetActive(true);
        }
        if (affiliationText != null)
        {
            affiliationText.text = unit.affiliation.ToString();
            affiliationText.gameObject.SetActive(true);
        }
        if (roleText != null)
        {
            roleText.text = unit.role.ToString();
            roleText.gameObject.SetActive(true);
        }

        // Hide player-only bullet icons
        if (bulletIcons != null)
            foreach (var img in bulletIcons)
                if (img != null) img.enabled = false;

        // Show attack panel
        if (attacksPanel != null) attacksPanel.SetActive(true);
        RefreshAttacks(unit);
        ClearAttackHighlights();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  INCREMENTAL UPDATES  (called each combat event)
    // ─────────────────────────────────────────────────────────────────────────

    public void SetMaxHP(int maxHP)
    {
        if (healthSlider != null)
            healthSlider.maxValue = Mathf.Max(1, maxHP);
    }

    public void SetHP(int currentHP)
    {
        if (healthSlider != null)
            healthSlider.value = Mathf.Clamp(currentHP, 0, healthSlider.maxValue);
        if (healthText != null)
            healthText.text = Mathf.Max(0, currentHP).ToString();
    }

    /// <summary>
    /// Update bullet icons. Uses bulletFullSprite / bulletSpentSprite if assigned;
    /// falls back to alpha fade if sprites are not set.
    /// </summary>
    public void SetBullets(int bullets, int maxBullets)
    {
        if (bulletIcons == null || bulletIcons.Length == 0) return;

        int clampedMax     = Mathf.Clamp(maxBullets, 0, bulletIcons.Length);
        int clampedBullets = Mathf.Clamp(bullets,    0, clampedMax);

        for (int i = 0; i < bulletIcons.Length; i++)
        {
            var img = bulletIcons[i];
            if (img == null) continue;

            if (i < clampedMax)
            {
                img.enabled = true;
                bool isFull = i < clampedBullets;

                if (isFull && bulletFullSprite != null)
                {
                    img.sprite = bulletFullSprite;
                    img.color  = Color.white;
                }
                else if (!isFull && bulletSpentSprite != null)
                {
                    img.sprite = bulletSpentSprite;
                    img.color  = Color.white;
                }
                else
                {
                    // Fallback: same sprite, dimmed when spent
                    img.color = isFull ? Color.white : new Color(1f, 1f, 1f, 0.25f);
                }
            }
            else
            {
                img.enabled = false;
            }
        }
    }

    /// <summary>
    /// Update status effect icons from a list of Sprites.
    /// Pass null or an empty list to clear all icons.
    /// </summary>
    public void SetStatusEffects(IList<Sprite> icons)
    {
        if (statusEffectIcons == null) return;
        int count = icons != null ? icons.Count : 0;

        for (int i = 0; i < statusEffectIcons.Length; i++)
        {
            var img = statusEffectIcons[i];
            if (img == null) continue;

            if (i < count && icons[i] != null)
            {
                img.sprite  = icons[i];
                img.enabled = true;
                img.color   = Color.white;
            }
            else
            {
                img.sprite  = null;
                img.enabled = false;
            }
        }
    }

    /// <summary>
    /// Convenience overload: pass the unit's active effect list directly.
    /// Extracts the icon from each StatusEffect automatically.
    /// </summary>
    public void SetStatusEffectsFromUnit(IList<StatusEffect> effects)
    {
        if (effects == null || effects.Count == 0) { SetStatusEffects(null); return; }
        var sprites = new List<Sprite>(effects.Count);
        foreach (var fx in effects)
            sprites.Add(fx.icon);
        SetStatusEffects(sprites);
    }

    /// <summary>
    /// Rebuild the attack panel to match the enemy's current attack list.
    /// Call whenever attacks change or after an enemy takes its turn.
    /// </summary>
    public void RefreshAttacks(EnemyUnit unit)
    {
        if (unit == null) return;

        // ── Attack 1 ──────────────────────────────────────────────────────────
        SetAttackSlot(0, unit.attacks,
                      attack1Name, attack1Damage, attack1EffectIcon);

        // ── Attack 2 ──────────────────────────────────────────────────────────
        SetAttackSlot(1, unit.attacks,
                      attack2Name, attack2Damage, attack2EffectIcon);

        // ── Attack 3 ──────────────────────────────────────────────────────────
        SetAttackSlot(2, unit.attacks,
                      attack3Name, attack3Damage, attack3EffectIcon);

        // ── Heal ──────────────────────────────────────────────────────────────
        if (unit.healMove != null)
        {
            if (healName   != null) healName.text   = unit.healMove.attackName;
            if (healAmount != null) healAmount.text = $"+{unit.healMove.healAmount} HP";
        }
        else
        {
            if (healName   != null) healName.text   = "—";
            if (healAmount != null) healAmount.text = "";
        }
    }

    /// <summary>
    /// Highlight the attack slot the enemy just chose.
    /// Pass attackIndex 0-2 for attacks, or -1 for the heal.
    /// Call after the enemy AI picks its move, before the animation plays.
    /// </summary>
    public void HighlightAttack(int attackIndex)
    {
        ClearAttackHighlights();

        Image target = attackIndex switch
        {
            0  => attack1Highlight,
            1  => attack2Highlight,
            2  => attack3Highlight,
            -1 => healHighlight,
            _  => null
        };

        if (target != null)
        {
            target.enabled = true;
            target.color   = new Color(1f, 0.85f, 0f, 0.35f); // warm gold flash
        }
    }

    public void ClearAttackHighlights()
    {
        foreach (var h in new[] { attack1Highlight, attack2Highlight, attack3Highlight, healHighlight })
            if (h != null) h.enabled = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    void SetAttackSlot(int index, List<EnemyAttack> attacks,
                       TextMeshProUGUI nameLabel,
                       TextMeshProUGUI damageLabel,
                       Image effectIcon)
    {
        bool hasAttack = attacks != null && index < attacks.Count && attacks[index] != null;

        if (nameLabel   != null)
            nameLabel.text   = hasAttack ? attacks[index].attackName : "—";

        if (damageLabel != null)
            damageLabel.text = hasAttack ? attacks[index].damage.ToString() : "";

        if (effectIcon  != null)
        {
            if (hasAttack && attacks[index].appliedEffect != StatusEffectType.None
                          && attacks[index].effectIcon != null)
            {
                effectIcon.sprite  = attacks[index].effectIcon;
                effectIcon.enabled = true;
                effectIcon.color   = Color.white;
            }
            else
            {
                effectIcon.enabled = false;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  EDITOR UTILITIES
    // ─────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    [ContextMenu("Clear Status Icons")]
    void EditorClearStatusIcons() => SetStatusEffects(null);

    [ContextMenu("Clear Bullets")]
    void EditorClearBullets()
    {
        if (bulletIcons == null) return;
        foreach (var img in bulletIcons)
            if (img != null) img.enabled = false;
    }

    [ContextMenu("Clear Attack Highlights")]
    void EditorClearHighlights() => ClearAttackHighlights();
#endif
}
