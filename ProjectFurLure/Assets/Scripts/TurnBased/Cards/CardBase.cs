using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Base")]
public class CardBase : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Name shown for the card")]
    public string cardName;

    [Header("Visual")]
    [Tooltip("Artwork / sprite for the card")]
    public Sprite cardImage;

    [Header("Gameplay")]
    [Tooltip("Bullets / focus cost to play this card")]
    public int bulletCost;

    [Tooltip("Primary damage / power value applied by this card")]
    public int cardDamage;

    [Tooltip("High-level card type (affects how the card behaves)")]
    public CardType cardType = CardType.Action;

    [TextArea(2, 6)]
    [Tooltip("Short description of what the card does")]
    public string description;

    [Header("Status Effects")]
    [Tooltip("Status effects this card applies when used (order is preserved).")]
    public List<StatusEffectType> statusEffects = new List<StatusEffectType>();
}

public enum CardType
{
    None,
    Action,
    Defense,
    Damage,
    Health,
    Combo
}

public enum StatusEffectType
{
    None,
    Physical,
    Fire,
    Poison,
    Oil,
    Weakness,
    Stun,
    Health
}
