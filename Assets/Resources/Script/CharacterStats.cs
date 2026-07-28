using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CampType
{
    Player,
    Enemy
}

public class CharacterStats : MonoBehaviour
{
    [Header("基础属性")]
    public CampType camp;
    public int maxHp = 100;
    public int attack = 20;
    public int moveCost = 3;
    public int attackRange = 2;
    public int speed = 100;

    public int currentHp;
    public bool IsDead => currentHp <= 0;

    public event Action<CharacterStats> OnDead;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if(IsDead)
        {
            return;
        }

        int gridX = Mathf.RoundToInt(transform.position.x);
        int gridY = Mathf.RoundToInt(transform.position.z);
        GridData currentGrid = GridManager.Instance.GetTile(gridX, gridY);
        int realDamage = Mathf.Max(1, damage - currentGrid.defBonus);

        currentHp = Mathf.Max(0, currentHp - realDamage);

        // 通知血条UI更新
        Blood blood = GetComponentInChildren<Blood>();
        if (blood != null)
        {
            blood.UpdateHp(currentHp, maxHp);
        }

        if (currentHp <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        // 释放占用的格子
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.z);
        GridManager.Instance.ResetMove(x, y);

        //广播死亡事件
        OnDead?.Invoke(this);
        GameManage.Instance.RemoveDeadUnit(this);

        Destroy(gameObject);
    }
}
