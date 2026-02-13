using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LawLure 
{
    LawLureBase _base;
    int level;

    public LawLure(LawLureBase LLbase, int LLlevel)
    {
        this._base = LLbase;
        this.level = LLlevel;
    }

    public int MaxHealth
    {
        get { return Mathf.FloorToInt((_base.maxHealth * level) / 100f) + 10; }
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((_base.attack * level) / 100f) + 5; }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt((_base.defense * level) / 100f) + 5; }
    }

    public int HardAttack
    {
        get { return Mathf.FloorToInt((_base.hardAttack * level) / 100f) + 5; }
    }

    public int HardDefense
    {
        get { return Mathf.FloorToInt((_base.hardDefense * level) / 100f) + 5; }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((_base.speedOfLL * level) / 100f) + 5; }
    }
}
