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

    public string nameofBandit { 
        get { return Name; } 
    } 
    
    public string description { 
        get { return Description; }
    }

    public Sprite image { 
            get { return Image; }
    }

    public BanditType bandittype { 
            get { return type; }
    }

    public GangType gang { 
            get { return Gang; }
    }

    public int maxHealth { 
            get { return MaxHealth; }
    }

    public int attack { 
            get { return Attack; }
    }

    public int defense { 
            get { return Defense; }
    }

    public int hardAttack { 
            get { return HardAttack; }
    }

    public int hardDefense { 
            get { return HardDefense; }
    }

    public int speedOfBandit { 
            get { return speed; }
    }
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
