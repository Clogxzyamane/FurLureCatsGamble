// ─────────────────────────────────────────────────────────────────────────────
//  EnemyData.cs
//
//  Data types only — no MonoBehaviour here.
//  EnemyUnit MonoBehaviour lives in EnemyUnit.cs (filename must match class name).
//
//  Contains:
//    • StatusEffect   — runtime effect instance (uses StatusEffectType from CardBase.cs)
//    • EnemyType      — faction enum
//    • EnemyAffiliation — group enum
//    • EnemyRole      — combat role enum
//    • EnemyAttack    — single attack/heal definition
// ─────────────────────────────────────────────────────────────────────────────

using UnityEngine;

// ── Runtime status effect instance ───────────────────────────────────────────
// StatusEffectType is declared in CardBase.cs — do not redeclare it here.

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int              remainingTurns;
    public int              damagePerTurn;
    public Sprite           icon;

    public StatusEffect() { }

    public StatusEffect(StatusEffectType type, int turns, int dot = 0, Sprite icon = null)
    {
        this.type           = type;
        this.remainingTurns = turns;
        this.damagePerTurn  = dot;
        this.icon           = icon;
    }
}

// ── Enemy classification enums ────────────────────────────────────────────────

public enum EnemyType
{
    LawLure,
    Bandit,
    Neutral
}

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

public enum EnemyRole
{
    None,
    Gunman,
    Sniper,
    Tank,
    Support,
    Archer,
    MiniBoss,
    Boss
}

// ── Enemy attack / heal definition ───────────────────────────────────────────

[System.Serializable]
public class EnemyAttack
{
    [Tooltip("Name shown on the enemy HUD attack panel.")]
    public string attackName = "Attack";

    [Tooltip("Damage dealt to the player. Set 0 for heal moves.")]
    public int damage = 5;

    [Tooltip("Status effect applied on hit (None = no effect).")]
    public StatusEffectType appliedEffect = StatusEffectType.None;

    [Tooltip("How many turns the status effect lasts.")]
    public int effectDuration = 2;

    [Tooltip("Damage per turn for DoT effects. Set 0 for Stun / Weakness.")]
    public int effectDotDamage = 1;

    [Tooltip("Sprite shown next to this attack on the HUD (optional).")]
    public Sprite effectIcon;

    [Tooltip("Check this ONLY on the Heal Move — not on attacks.")]
    public bool isHeal = false;

    [Tooltip("HP restored when isHeal is true.")]
    public int healAmount = 0;
}
