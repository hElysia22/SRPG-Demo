using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManage : MonoBehaviour
{
    public List<GameObject> Players = new List<GameObject>();
    public static GameManage Instance;
    public int currentIndex = 0;
    public Button btn1;
    public Button btn2;
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        btn1.gameObject.SetActive(false);
        btn2.gameObject.SetActive(false);
        StartCurrentTurn();
    }

    public void StartCurrentTurn()
    {
        var move = Players[currentIndex].GetComponent<Move>();
        if (move != null && move.enabled)
        {
            move.CalculateMoveableGrid();
            btn1.gameObject.SetActive(true);
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

    public void EndMoveTurn()
    {
        var move = Players[currentIndex].GetComponent<Move>();
        if (move != null && move.enabled)
        {
            move.canMove = false;
            btn1.gameObject.SetActive(false);
        }
    }

    public void StartAttackTurn()
    {
        var attack = Players[currentIndex].GetComponent<Attack>();
        if (attack != null && attack.enabled)
        {
            attack.CalculateAttackableGrid();
            btn2.gameObject.SetActive(true);
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

    public void EndAttackTurn()
    {
        var attack = Players[currentIndex].GetComponent<Attack>();
        if (attack != null && attack.enabled)
        {
            attack.canAttack = false;
            btn2.gameObject.SetActive(false);
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
            btn1.gameObject.SetActive(false);
            btn2.gameObject.SetActive(false);
        }
        currentIndex++;
        if(currentIndex >= Players.Count)
        {
            currentIndex = 0;
        }
        StartCurrentTurn();
    }

}
