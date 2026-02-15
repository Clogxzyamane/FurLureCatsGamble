using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Combatstats
{
    public int maxHealth;
    public int currentHealth;
    public int attackPower;
    public int defensePower; 
    public List<string> multipliers; // List to hold multipliers like "Critical Hit", "Weakness", etc.
    public Vector2 DamageRange; // Range for random damage calculation (min, max)

    public float EvaluateDamage()
    {
        // Apply multipliers (for simplicity, let's assume each multiplier adds 10% damage)
        float multiplierValue = 1f;
        foreach (string multiplier in multipliers)
        {
            multiplierValue += 0.1f; // Each multiplier adds 10%
        }

        // Randomly calculate damage within the specified range
        float baseDamage = Random.Range(DamageRange.x, DamageRange.y);
        return baseDamage * multiplierValue;
    }
}
