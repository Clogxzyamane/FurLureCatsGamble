// ─────────────────────────────────────────────────────────────────────────────
//  CardBase.cs
//
//  Single file that defines the entire card system.
//
//  To create a card in Unity:
//    Right-click in the Project window → Cards → Card Base
//    Fill in cardName, cardDamage, cardType, statusEffects, etc.
//    Drag the asset into TurnBasedSystem's Deck list in the Inspector.
//
//  Owns:
//    • StatusEffectType  — master list of every effect in the game
//    • CardType          — categories a card can belong to
//    • CardBase          — the ScriptableObject you author one asset per card
//
//  TurnBasedSystem uses CardBase directly:
//    [SerializeField] List<CardBase> deck;
//    Reads: cardName, cardDamage, bulletCost, cardType, statusEffects
//
//  DO NOT declare StatusEffectType anywhere else in the project.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine;

// ── Status effect types — MASTER LIST ────────────────────────────────────────

/// <summary>
/// Every status effect in the game.
/// Referenced by CardBase, EnemyAttack, StatusEffect, PlayerUnit, EnemyUnit.
/// Declared ONLY here — do not redeclare in any other file.
/// </summary>
public enum StatusEffectType
{
    None,
    Physical,
    Fire,
    Poison,
    Oil,
    Weakness,
    Stun,
    Health      // healing-over-time / regen effect
}

// ── Card type categories ──────────────────────────────────────────────────────

/// <summary>
/// High-level category that determines how a card behaves and is displayed.
/// </summary>
public enum CardType
{
    None,
    Action,
    Defense,
    Damage,
    Health,
    Combo
}

// ── CardBase — the card asset ─────────────────────────────────────────────────

/// <summary>
/// ScriptableObject that represents one card in the game.
///
/// Workflow:
///   1. Right-click Project → Cards → Card Base  to make a new card asset.
///   2. Fill in all fields in the Inspector.
///   3. Drag the asset into TurnBasedSystem → Deck in the Inspector.
///
/// TurnBasedSystem reads cardName, cardDamage, bulletCost, cardType,
/// and statusEffects directly from this asset at runtime.
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Base")]
public class CardBase : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    [Tooltip("Name shown on the card button and in the dialogue log.")]
    public string cardName = "New Card";

    [Header("Visual")]
    [Tooltip("Card artwork shown in the card UI.")]
    public Sprite cardImage;

    // ── Gameplay ──────────────────────────────────────────────────────────────

    [Header("Gameplay")]
    [Tooltip("Bullets / focus spent when this card is played (0 = costs nothing).")]
    public int bulletCost = 0;

    [Tooltip("Base damage dealt to the target when this card resolves.")]
    public int cardDamage = 5;

    [Tooltip("Card category — affects visual style and future game logic.")]
    public CardType cardType = CardType.Damage;

    [TextArea(2, 5)]
    [Tooltip("Flavour / rules text shown on the card detail panel.")]
    public string description;

    // ── Status effects applied on hit ─────────────────────────────────────────

    [Header("Status Effects Applied on Hit")]
    [Tooltip("One or more effects applied to the target when this card hits. " +
             "Leave empty for a plain damage card.")]
    public List<StatusEffectType> statusEffects = new List<StatusEffectType>();

    [Tooltip("How many turns each status effect listed above lasts.")]
    public int effectDuration = 2;

    [Tooltip("Damage-per-turn for DoT effects (Fire, Poison, Oil). " +
             "Ignored for non-DoT effects like Stun or Weakness.")]
    public int effectDotDamage = 1;

    [Tooltip("Optional icon shown next to the status effect on the HUD. " +
             "Assign a Sprite that matches the effect visually.")]
    public Sprite effectIcon;
}
