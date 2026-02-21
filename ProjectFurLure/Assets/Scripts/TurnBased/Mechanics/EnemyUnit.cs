// ─────────────────────────────────────────────────────────────────────────────
//  EnemyUnit.cs
//
//  MonoBehaviour component attached to every enemy prefab.
//  Filename matches class name — required by Unity to find it in Add Component.
//
//  Depends on:  EnemyData.cs  (enums + EnemyAttack + StatusEffect)
//               CardBase.cs   (StatusEffectType)
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine;

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
