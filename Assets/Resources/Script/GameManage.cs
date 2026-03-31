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
        if (move != null)
        {
            move.canPlay = true;
        }
    }

    public void EndTurn()
    {
        var move = Players[currentIndex].GetComponent<Move>();
        if (move != null)
        {
            move.canPlay = false;
        }
        currentIndex++;
        if(currentIndex >= Players.Count)
        {
            currentIndex = 0;
        }
        StartCurrentTurn();
    }

}
