using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bandit Move", menuName = "Characters/Bandit Move Base")]

public class BanditMovesbase : ScriptableObject
{
    [SerializeField]string Name;
    [TextArea]
    [SerializeField] string Description;

    [SerializeField] BDamageType type;
    [SerializeField] int power;
    [SerializeField] float DodgeChance;

    public string GetName()
    {
        return Name;
    }

    public string description
    {
        get { return Description; }
    }

    public BDamageType damageType
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public float dodgeChance
    {
        get { return DodgeChance; }
    }


}

public enum BDamageType
{
    None,
    physical,
    Fire,
    Poison,
    Oil,
    Catnip,
    weakness,
    Stun

}
