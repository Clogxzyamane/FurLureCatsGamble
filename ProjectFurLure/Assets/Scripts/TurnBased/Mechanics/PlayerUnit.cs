using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    LawLure,
    Bandits,
    Neutral
}

public enum Affiliation
{
    None,
    Law,
    Lure,
    Gang1,
    Gang2,
    Gang3,
    Indigenous
}

public enum EnemyRole
{
    Gunmen,
    Sniper,
    Tank,
    Support,
    MiniBoss,
    Boss,
    None
}

public class PlayerUnit : MonoBehaviour
{
    [Header("Identity")]
    public string unitName;
    [Tooltip("If true this unit is considered an enemy")]
    public bool isEnemy = false;
    [Tooltip("High-level enemy faction/type")]
    public EnemyType enemyType = EnemyType.Neutral;
    [Tooltip("Detailed affiliation (Law/Lure/Gang/Indigenous). Use None for player or neutral.")]
    public Affiliation affiliation = Affiliation.None;
    [Tooltip("Role/class of the enemy")]
    public EnemyRole enemyRole = EnemyRole.None;

    [Header("Combat")]
    [Tooltip("Base damage this unit deals with its attacks")]
    public int baseDamage;
    [Tooltip("Runtime damage (may include buffs/debuffs)")]
    public int damage;
    [Tooltip("Max health")]
    public int maxHP;
    [Tooltip("Current health")]
    public int currentHP;

    [Header("Status Effects")]
    [Tooltip("Simple string identifiers for status effects; map these to sprites in UI")]
    public List<string> statusEffects = new List<string>();

    // Apply damage and return true if unit died as a result.
    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            return true;
        }
        return false;
    }

    // Heal and clamp to maxHP.
    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    // Status effect helpers
    public void AddStatusEffect(string effect)
    {
        if (string.IsNullOrEmpty(effect)) return;
        if (!statusEffects.Contains(effect))
            statusEffects.Add(effect);
    }

    public void RemoveStatusEffect(string effect)
    {
        if (string.IsNullOrEmpty(effect)) return;
        statusEffects.Remove(effect);
    }
}
