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

    public string nameofLL
    {
        get { return Name; }
    }

    public string description
    {
        get { return Description; }
    }

    public Sprite image
    {
        get { return Image; }
    }

    public EnemyType LLtype
    {
        get { return type; }
    }

    public LawLureType organisation
    {
        get { return Organisation; }
    }

    public int maxHealth
    {
        get { return MaxHealth; }
    }

    public int attack
    {
        get { return Attack; }
    }

    public int defense
    {
        get { return Defense; }
    }

    public int hardAttack
    {
        get { return HardAttack; }
    }

    public int hardDefense
    {
        get { return HardDefense; }
    }

    public int speedOfLL
    {
        get { return speed; }
    }
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
