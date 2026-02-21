// ─────────────────────────────────────────────────────────────────────────────
//  EnemyData.cs
//
//  Contains all enemy-specific types and the EnemyUnit MonoBehaviour.
//
//  Contains:
//    • StatusEffect class     (runtime effect instance — uses StatusEffectType
//                              declared in CardBase.cs)
//    • EnemyType enum
//    • EnemyAffiliation enum
//    • EnemyRole enum
//    • EnemyAttack class
//    • EnemyUnit MonoBehaviour
//
//  NOTE: StatusEffectType is declared in CardBase.cs — do NOT redeclare it here.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine;

// ── Runtime status effect instance ───────────────────────────────────────────
// StatusEffectType enum lives in CardBase.cs and is the master list for the game.

/// <summary>
/// A runtime status effect instance attached to a unit.
/// Shared between PlayerUnit and EnemyUnit.
/// Uses StatusEffectType from CardBase.cs.
/// </summary>
[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int              remainingTurns;
    public int              damagePerTurn;  // DoT per tick (fire / poison / bleed)
    public Sprite           icon;           // icon shown on the HUD

    public StatusEffect() { }

    public StatusEffect(StatusEffectType type, int turns, int dot = 0, Sprite icon = null)
    {
        this.type           = type;
        this.remainingTurns = turns;
        this.damagePerTurn  = dot;
        this.icon           = icon;
    }
}

// ── Enemy classification ──────────────────────────────────────────────────────

/// <summary>High-level faction/type of an enemy.</summary>
public enum EnemyType
{
    LawLure,
    Bandit,     // was "Bandits" in original PlayerUnit — unified here
    Neutral
}

/// <summary>Specific group an enemy belongs to.</summary>
public enum EnemyAffiliation
{
    None,
    Law,
    Lure,
    Gang1,
    Gang2,
    Gang3,
    Indigenous
}

/// <summary>Combat role / class of an enemy.</summary>
public enum EnemyRole
{
    None,
    Gunman,     // was "Gunmen" in original PlayerUnit — unified here
    Sniper,
    Tank,
    Support,
    Archer,
    MiniBoss,
    Boss
}

// ── Enemy attack / heal definition ───────────────────────────────────────────

/// <summary>
/// One attack (or the heal move) that an enemy can use on its turn.
/// Assign up to 3 of these + 1 heal on each EnemyUnit in the Inspector.
/// </summary>
[System.Serializable]
public class EnemyAttack
{
    [Tooltip("Name shown on the enemy HUD attack panel.")]
    public string attackName = "Attack";

    [Tooltip("Damage dealt to the player. Set 0 for a pure-effect or heal move.")]
    public int damage = 5;

    [Tooltip("Status effect applied on hit (None = no effect).")]
    public StatusEffectType appliedEffect = StatusEffectType.None;

    [Tooltip("How many turns the applied status effect lasts.")]
    public int effectDuration = 2;

    [Tooltip("Damage-per-turn for DoT effects (fire, poison, bleed).")]
    public int effectDotDamage = 1;

    [Tooltip("Sprite shown next to this attack on the HUD (optional).")]
    public Sprite effectIcon;

    [Tooltip("True = this entry is a heal move, not a damage move.")]
    public bool isHeal = false;

    [Tooltip("HP restored when isHeal is true (heals self or an ally).")]
    public int healAmount = 0;
}

// ── EnemyUnit MonoBehaviour ───────────────────────────────────────────────────

/// <summary>
/// Component added to every enemy GameObject.
/// TurnBasedSystem works exclusively with EnemyUnit for enemies —
/// PlayerUnit is reserved for the player character only.
/// </summary>
[DisallowMultipleComponent]
public class EnemyUnit : MonoBehaviour
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    [Tooltip("Display name shown on the enemy HUD.")]
    public string unitName = "Enemy";

    [Tooltip("High-level faction.")]
    public EnemyType enemyType = EnemyType.Bandit;

    [Tooltip("Specific group affiliation.")]
    public EnemyAffiliation affiliation = EnemyAffiliation.None;

    [Tooltip("Combat role / class.")]
    public EnemyRole role = EnemyRole.Gunman;

    // ── Stats ─────────────────────────────────────────────────────────────────

    [Header("Stats")]
    public int maxHP      = 20;
    public int currentHP  = 20;
    public int baseDamage = 5;

    [Tooltip("Runtime damage — may differ from baseDamage when buffs/debuffs are active.")]
    public int damage = 5;

    // ── Attacks ───────────────────────────────────────────────────────────────

    [Header("Attacks  (add exactly 3 entries)")]
    [Tooltip("The 3 attacks this enemy may choose from on its turn.")]
    public List<EnemyAttack> attacks = new List<EnemyAttack>();

    [Header("Heal Move")]
    [Tooltip("The enemy's heal action. Used when HP is low.")]
    public EnemyAttack healMove = new EnemyAttack
    {
        attackName = "Heal",
        isHeal     = true,
        healAmount = 5
    };

    // ── Runtime status effects ────────────────────────────────────────────────

    [Header("Active Status Effects  (runtime — do not set in Inspector)")]
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    // ─────────────────────────────────────────────────────────────────────────
    //  COMBAT API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Apply damage. Returns true if the enemy dies.</summary>
    public bool TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        return currentHP <= 0;
    }

    /// <summary>Restore HP, clamped to maxHP. Returns new currentHP.</summary>
    public int Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        return currentHP;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  STATUS EFFECT API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Add a status effect or refresh its duration if already present.
    /// After calling this, refresh the enemy HUD:
    ///     enemyHUD.SetStatusEffectsFromUnit(enemyUnit.activeEffects);
    /// </summary>
    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect == null) return;
        var existing = activeEffects.Find(e => e.type == effect.type);
        if (existing != null)
        {
            existing.remainingTurns = Mathf.Max(existing.remainingTurns, effect.remainingTurns);
            return;
        }
        activeEffects.Add(effect);
    }

    /// <summary>Remove all effects of the given type.</summary>
    public void RemoveStatusEffect(StatusEffectType type)
    {
        activeEffects.RemoveAll(e => e.type == type);
    }

    /// <summary>Returns true if this enemy currently has the given effect.</summary>
    public bool HasStatusEffect(StatusEffectType type)
    {
        return activeEffects.Exists(e => e.type == type);
    }

    /// <summary>
    /// Tick all DoT effects at the start of this enemy's turn.
    /// Returns total HP lost from DoT. Removes expired effects automatically.
    /// Called by TurnBasedSystem at the start of each enemy's phase.
    /// </summary>
    public int TickStatusEffects()
    {
        int totalDot = 0;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var fx = activeEffects[i];
            totalDot += fx.damagePerTurn;
            fx.remainingTurns--;
            if (fx.remainingTurns <= 0)
                activeEffects.RemoveAt(i);
        }
        currentHP = Mathf.Max(0, currentHP - totalDot);
        return totalDot;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  AI HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Simple AI: returns an attack index (0-2), or -1 to use the heal move.
    /// Override or extend with role-specific logic as needed.
    /// </summary>
    public int ChooseAttackIndex()
    {
        if (attacks == null || attacks.Count == 0) return -1;

        // If low on HP, 50% chance to heal instead
        if (currentHP < maxHP * 0.3f && healMove != null && Random.value < 0.5f)
            return -1;

        return Random.Range(0, Mathf.Min(attacks.Count, 3));
    }
}
