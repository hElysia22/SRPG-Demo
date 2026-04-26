using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public static EndTurnButton Instance;
    void Awake()
    {
        Instance = this;
    }
    public void EndTurn()
    {
        GameManage.Instance.EndAttackTurn();
        GameManage.Instance.EndTurn();
    }

    public void EndMoveTurn()
    {
        GameManage.Instance.EndMoveTurn();
        GameManage.Instance.StartAttackTurn();
    }
}
