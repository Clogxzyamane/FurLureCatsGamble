using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TurnState

{
    Start,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

public class TurnBasedSystem : MonoBehaviour


{
    public GameObject player;
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;

    public Transform playerPoint;
    public Transform enemyPoint1;
    public Transform enemyPoint2;
    public Transform enemyPoint3;


    PlayerUnit playerUnit;
    BanditsBase BanditUnit;



    public TurnState State;

    void Start()
    {
        State = TurnState.Start;
        SetupFight();
    }

    void SetupFight()
    {
        GameObject PlayerGO = Instantiate(player, playerPoint);
        PlayerGO.GetComponent<PlayerUnit>();

        GameObject BanditGO1 = Instantiate(enemy1, enemyPoint1);
        BanditGO1.GetComponent<BanditsBase>();


        GameObject BanditGO2 = Instantiate(enemy2, enemyPoint2);
        BanditGO2.GetComponent<BanditsBase>();

        GameObject BanditGO3 = Instantiate(enemy3, enemyPoint3);
        BanditGO3.GetComponent<BanditsBase>();
    }

}


