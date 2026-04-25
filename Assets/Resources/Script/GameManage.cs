using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManage : MonoBehaviour
{
    public List<GameObject> Players = new List<GameObject>();
    public static GameManage Instance;
    public int currentIndex = 0;
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCurrentTurn();
    }

    public void StartCurrentTurn()
    {
        var move = Players[currentIndex].GetComponent<Move>();
        if (move != null && move.enabled)
        {
            move.CalculateMoveableGrid();
            move.canMove = true;
        }
        else
        {
            currentIndex++;
            if (currentIndex >= Players.Count)
            {
                currentIndex = 0;
            }
            StartCurrentTurn();
        }
    }

    public void StartAttackTurn()
    {
        var move = Players[currentIndex].GetComponent<Move>();
        if (move != null && move.enabled)
        {
            move.canMove = false;
        }
        var attack = Players[currentIndex].GetComponent<Attack>();
        if (attack != null && attack.enabled)
        {
            attack.CalculateAttackableGrid();
            attack.canAttack = true;
        }
        else
        {
            currentIndex++;
            if (currentIndex >= Players.Count)
            {
                currentIndex = 0;
            }
            StartCurrentTurn();
        }
    }

    public void EndTurn()
    {
        var move = Players[currentIndex].GetComponent<Move>();
        var attack = Players[currentIndex].GetComponent<Attack>();
        if (move != null && move.enabled || attack != null && attack.enabled)
        {
            move.canMove = false;
            attack.canAttack = false;
        }
        currentIndex++;
        if(currentIndex >= Players.Count)
        {
            currentIndex = 0;
        }
        StartCurrentTurn();
    }

}
