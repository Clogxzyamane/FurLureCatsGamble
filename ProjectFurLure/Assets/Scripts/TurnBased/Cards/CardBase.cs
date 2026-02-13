using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Base")]

public class CardBase : ScriptableObject
{
    [SerializeField] string cardName;
    [SerializeField] Sprite cardImage;
    [SerializeField] int bulletCost;
    [SerializeField] string description;
    [SerializeField] CardType cardType;
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
