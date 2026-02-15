using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLevel : MonoBehaviour
{
    [Header("Leveling")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNext = 100;
    [Tooltip("Multiplier applied to xpToNext after each level up (e.g. 1.5 increases required XP).")]
    public float xpGrowth = 1.5f;

    [Header("Stats")]
    [Tooltip("Strength increases card damage (multiplier).")]
    public int strength = 0;
    [Tooltip("Extra max HP gained from levels (added to PlayerUnit.maxHP).")]
    public int bonusHealth = 0;
    [Tooltip("Speed increases dodge effectiveness.")]
    public int speed = 0;

    [Tooltip("Damage multiplier per Strength point (e.g. 0.05 = +5% per STR).")]
    public float damagePerStrength = 0.05f;
    [Tooltip("Dodge bonus per Speed point (additive to base dodge chance).")]
    public float dodgePerSpeed = 0.02f;
    [Tooltip("Health increase per level when leveling up.")]
    public int healthPerLevel = 5;
    [Tooltip("Strength increase per level (example).")]
    public int strengthPerLevel = 1;
    [Tooltip("Speed increase per level (example).")]
    public int speedPerLevel = 1;

    /// <summary>
    /// Grant XP and handle level-ups.
    /// </summary>
    public void GainXP(int amount)
    {
        if (amount <= 0) return;
        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            LevelUp();
            xpToNext = Mathf.CeilToInt(xpToNext * xpGrowth);
        }
    }

    void LevelUp()
    {
        level++;
        // example progression
        strength += strengthPerLevel;
        speed += speedPerLevel;
        bonusHealth += healthPerLevel;

        // If PlayerUnit exists on same GameObject, apply immediate health increase
        var pu = GetComponent<PlayerUnit>();
        if (pu != null)
        {
            pu.maxHP += healthPerLevel;
            pu.currentHP = Mathf.Min(pu.currentHP + healthPerLevel, pu.maxHP);
            // update base damage if you want strength to directly add to base damage:
            // pu.baseDamage += strengthPerLevel;
            pu.damage = pu.baseDamage; // ensure runtime damage resets from base
        }

        Debug.Log($"Level Up! New level: {level}, STR:{strength}, SPD:{speed}, HP+:{bonusHealth}");
        // TODO: raise UnityEvent for UI/VFX.
    }

    /// <summary>
    /// Returns multiplicative damage multiplier for card damage.
    /// Example: STR 2 with damagePerStrength 0.05 returns 1.10 (10% more).
    /// </summary>
    public float GetDamageMultiplier()
    {
        return 1f + (strength * damagePerStrength);
    }

    /// <summary>
    /// Returns additive dodge chance bonus (0..1).
    /// Example: SPD 3 with dodgePerSpeed 0.02 returns 0.06 (6% extra dodge chance).
    /// </summary>
    public float GetDodgeChanceBonus()
    {
        return speed * dodgePerSpeed;
    }
}
