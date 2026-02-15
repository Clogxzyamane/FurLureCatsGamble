using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TurnState { Start, PlayerTurn, EnemyTurn, Victory, Defeat }



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
    PlayerUnit enemyUnit1;
    PlayerUnit enemyUnit2;
    PlayerUnit enemyUnit3;

    public TextMeshProUGUI dialogue;

    public TurnBasedHUD playerHUD;
    public TurnBasedHUD enemy1HUD;
    public TurnBasedHUD enemy2HUD;
    public TurnBasedHUD enemy3HUD;


    public TurnState State;

    void Start()
    {
        State = TurnState.Start;
        StartCoroutine(SetupFight());
    }

    IEnumerator SetupFight()
    {
        GameObject PlayerGO = Instantiate(player, playerPoint);
        PlayerGO.GetComponent<PlayerUnit>();


        GameObject EnemyGO1 = Instantiate(enemy1, enemyPoint1);
        EnemyGO1.GetComponent<PlayerUnit>();



        GameObject EnemyGO2 = Instantiate(enemy2, enemyPoint2);
        EnemyGO2.GetComponent<PlayerUnit>();

        

        GameObject EnemyGO3 = Instantiate(enemy3, enemyPoint3);
        EnemyGO3.GetComponent<PlayerUnit>();

        


        playerHUD.SetHUD(playerUnit);
        enemy1HUD.SetHUD(enemyUnit1);
        enemy2HUD.SetHUD(enemyUnit2);
        enemy3HUD.SetHUD(enemyUnit3);

        yield return new WaitForSeconds(2f);
        State = TurnState.PlayerTurn;
        PlayerTurn();

    }

    IEnumerator PlayerTurn()
    {
        dialogue.text = "Play your cards";
        yield return new WaitForSeconds(2f);
        //Attack
        bool isDead = enemyUnit1.TakeDamage(playerUnit.damage);
        enemy1HUD.SetHP(enemyUnit1.currentHP);
        dialogue.text = "You attack " + enemyUnit1.unitName + " for " + playerUnit.damage + " damage";
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
            State = TurnState.Victory;
            EndFight();
        }
        else
        {
            State = TurnState.EnemyTurn;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {
        dialogue.text = enemyUnit1.unitName + " attacks!";
        yield return new WaitForSeconds(1f);
        bool isDead = playerUnit.TakeDamage(enemyUnit1.damage);
        playerHUD.SetHP(playerUnit.currentHP);
        dialogue.text = enemyUnit1.unitName + " attacks you for " + enemyUnit1.damage + " damage";
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
            State = TurnState.Defeat;
            EndFight();
        }
        else
        {
            State = TurnState.PlayerTurn;
            StartCoroutine(PlayerTurn());
        }
    }

    void EndFight()
    {
        if (State == TurnState.Victory)
        {
            dialogue.text = "You won!";
        }
        else if (State == TurnState.Defeat)
        {
            dialogue.text = "You Lost!";
        }
    }

    public void OnAttackButton()
    {
        if (State != TurnState.PlayerTurn)
            return;
        StartCoroutine(PlayerTurn());
    }

}


