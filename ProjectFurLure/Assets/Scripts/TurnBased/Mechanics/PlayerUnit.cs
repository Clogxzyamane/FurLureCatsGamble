// ─────────────────────────────────────────────────────────────────────────────
//  PlayerUnit.cs
//
//  Represents the PLAYER character (Echo) in combat.
//
//  What changed vs the original:
//  ─ REMOVED: EnemyType, Affiliation, EnemyRole enum declarations
//             → These now live exclusively in EnemyData.cs (no duplicates)
//  ─ REMOVED: isEnemy, enemyType, affiliation, enemyRole fields
//             → Enemy identity data belongs on EnemyUnit (EnemyData.cs)
//  ─ ADDED:   List<StatusEffect> activeEffects
//             → Uses StatusEffect from EnemyData.cs and StatusEffectType from
//               CardBase.cs so TurnBasedHUD.SetStatusEffectsFromUnit() works
//  ─ KEPT:    unitName, baseDamage, damage, maxHP, currentHP
//             → TurnBasedSystem and TurnBasedHUD read all of these
//  ─ KEPT:    TakeDamage, Heal, AddStatusEffect, RemoveStatusEffect
//             → Same API surface TurnBasedSystem already calls
//
//  Dependencies (must be in the same Unity project):
//    • CardBase.cs   — provides StatusEffectType (master enum)
//    • EnemyData.cs  — provides StatusEffect (runtime instance class)
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerUnit : MonoBehaviour
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    [Tooltip("Display name shown on the player HUD. Set this to 'Echo'.")]
    public string unitName = "Echo";

    // ── Combat stats ──────────────────────────────────────────────────────────

    [Header("Combat")]
    [Tooltip("Base damage this unit deals. TurnBasedSystem reads this for free-shot and card multiplier.")]
    public int baseDamage = 5;

    [Tooltip("Runtime damage — may differ from baseDamage when buffs/debuffs are active.")]
    public int damage = 5;

    [Tooltip("Maximum health points.")]
    public int maxHP = 100;

    [Tooltip("Current health — updated at runtime by TakeDamage / Heal.")]
    public int currentHP = 100;

    // ── Status effects ────────────────────────────────────────────────────────

    [Header("Status Effects  (runtime)")]
    [Tooltip("Active status effects on the player. Populated at runtime via ApplyStatusEffect.")]
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    // ─────────────────────────────────────────────────────────────────────────
    //  COMBAT API  (called by TurnBasedSystem)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Apply damage to the player. Returns true if the player dies (HP reaches 0).
    /// Called by TurnBasedSystem.EnemyAttackSequence.
    /// </summary>
    public bool TakeDamage(int dmg)
    {
        currentHP = Mathf.Max(0, currentHP - dmg);
        return currentHP <= 0;
    }

    /// <summary>
    /// Restore HP, clamped to maxHP.
    /// </summary>
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  STATUS EFFECT API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Add a StatusEffect or refresh its duration if already present.
    /// After calling this, refresh the player HUD:
    ///     playerHUD.SetStatusEffectsFromUnit(playerUnit.activeEffects);
    /// </summary>
    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect == null) return;

        var existing = activeEffects.Find(e => e.type == effect.type);
        if (existing != null)
        {
            // Refresh: keep the longer duration
            existing.remainingTurns = Mathf.Max(existing.remainingTurns, effect.remainingTurns);
            return;
        }
        activeEffects.Add(effect);
    }

    /// <summary>
    /// Remove all effects of the given type.
    /// </summary>
    public void RemoveStatusEffect(StatusEffectType type)
    {
        activeEffects.RemoveAll(e => e.type == type);
    }

    /// <summary>
    /// Legacy string-based remove (kept for backward compatibility).
    /// Tries to parse the string as a StatusEffectType enum value.
    /// </summary>
    public void RemoveStatusEffect(string effectName)
    {
        if (System.Enum.TryParse(effectName, true, out StatusEffectType parsed))
            RemoveStatusEffect(parsed);
    }

    /// <summary>
    /// Legacy string-based add (kept for backward compatibility).
    /// Creates a 1-turn, 0-damage effect with no icon.
    /// Prefer ApplyStatusEffect(StatusEffect) for full control.
    /// </summary>
    public void AddStatusEffect(string effectName)
    {
        if (string.IsNullOrEmpty(effectName)) return;
        if (System.Enum.TryParse(effectName, true, out StatusEffectType parsed))
            ApplyStatusEffect(new StatusEffect(parsed, turns: 1));
    }

    /// <summary>
    /// Tick all DoT effects (fire, poison, bleed) at the start of the player's turn.
    /// Returns total damage taken. Removes expired effects automatically.
    /// Call this in TurnBasedSystem at the start of PlayerTurn if you want DoT on the player.
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

    /// <summary>
    /// Returns true if the player currently has the given effect active.
    /// </summary>
    public bool HasStatusEffect(StatusEffectType type)
    {
        return activeEffects.Exists(e => e.type == type);
    }
}
