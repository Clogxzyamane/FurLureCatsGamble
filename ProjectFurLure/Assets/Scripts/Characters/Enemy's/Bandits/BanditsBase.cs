using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bandit", menuName = "Characters/Bandit Base")]

public class BanditsBase : ScriptableObject
{
    [SerializeField] string Name;
    [TextArea]
    [SerializeField] string Description;
    [SerializeField] Sprite Image;
    [SerializeField] BanditType type;
    [SerializeField] GangType Gang;

    [SerializeField] int MaxHealth;
    [SerializeField] int Attack;
    [SerializeField] int Defense;
    [SerializeField] int HardAttack;
    [SerializeField] int HardDefense;
    [SerializeField] int speed;
}
public enum BanditType
{
    None,
    gunmen,
    Sniper,
    Tank,
    Support,
    MiniBoss,
    Boss
}

public enum GangType
{
    None,
    Gang1,
    Gang2,
    Gang3,
    Indigenous

}