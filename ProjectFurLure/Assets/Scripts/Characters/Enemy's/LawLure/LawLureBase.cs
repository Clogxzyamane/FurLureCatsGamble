using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LawLure", menuName = "Characters/LawLure Base")]

public class LawLureBase : ScriptableObject
{
    [SerializeField] string Name;
    [TextArea]
    [SerializeField] string Description;
    [SerializeField] Sprite Image;
    [SerializeField] EnemyType type;
    [SerializeField] LawLureType Organisation;

    [SerializeField] int MaxHealth;
    [SerializeField] int Attack;
    [SerializeField] int Defense;
    [SerializeField] int HardAttack;
    [SerializeField] int HardDefense;
    [SerializeField] int speed;
}
public enum EnemyType
{
    None,
    gunmen,
    Sniper,
    Tank,
    Support,
    MiniBoss,
    Boss
}

public enum LawLureType
{
    None,
    Law,
    Lure
}
