using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LawLure Move", menuName = "Characters/LawLure Move Base")]
public class LawLureMovesBase : ScriptableObject
{
    [SerializeField] string Name;
    [TextArea]
    [SerializeField] string Description;

    [SerializeField] LLDamageType type;
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

    public LLDamageType damageType
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

public enum LLDamageType
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
