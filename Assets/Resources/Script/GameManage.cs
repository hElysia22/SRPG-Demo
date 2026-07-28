using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 单位回合内的阶段
public enum UnitPhase
{
    Idle,       // 未行动
    Move,       // 移动阶段
    Attack,     // 攻击阶段
    Ended       // 行动结束
}

public class GameManage : MonoBehaviour
{
    public static GameManage Instance;

    [Header("参战单位")]
    public List<CharacterStats> allUnits = new List<CharacterStats>();

    [Header("行动条参数")]
    public float barThreshold = 10000f;

    // 当前行动单位与阶段
    [SerializeField] private CharacterStats _currentUnit;
    public CharacterStats CurrentUnit => _currentUnit;
    public UnitPhase CurrentPhase = UnitPhase.Idle;

    // 全局事件
    public event Action<CharacterStats> OnUnitTurnStart;
    public event Action<CharacterStats, UnitPhase> OnUnitPhaseChanged;
    public event Action<CharacterStats> OnUnitTurnEnd;

    // 内部数据
    private Dictionary<CharacterStats, float> _barValues = new();
    private bool _isActing = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 初始化所有单位行动条
        foreach (var unit in allUnits)
        {
            if (unit != null && !unit.IsDead)
            {
                _barValues[unit] = 0f;
            }
        }
        AdvanceActionBar();
    }

    /// 推进行动条：累加所有单位进度 → 选出下一个行动单位 → 开启回合
    private void AdvanceActionBar()
    {
        StartCoroutine(AdvanceActionBarCoroutine());
    }

    private IEnumerator AdvanceActionBarCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        while (true)
        {
            // 正在行动，跳过本次推进
            if (_isActing)
                continue; 

            CharacterStats topUnit = null;
            float maxValue = -1f;

            foreach (var kvp in _barValues)
            {
                // 这里continue完全合法
                if (kvp.Key == null || kvp.Key.IsDead)
                    continue;

                if (kvp.Value >= barThreshold && kvp.Value > maxValue)
                {
                    maxValue = kvp.Value;
                    topUnit = kvp.Key;
                }
            }

            if (topUnit != null)
            {
                _barValues[topUnit] -= barThreshold;
                StartUnitTurn(topUnit);
                yield break; // 找到行动单位，终止协程
            }

            // 无单位达标，全体叠加速度，进入下一次循环等待
            foreach (var unit in allUnits)
            {
                if (unit == null || unit.IsDead)
                    continue; 
                if (!_barValues.ContainsKey(unit))
                    _barValues[unit] = 0;
                _barValues[unit] += unit.speed;
            }
        }
    }

    /// 立即行动：让指定单位成为下一个行动者（不打断当前回合）
    public void ForceNextAction(CharacterStats targetUnit)
    {
        if (targetUnit == null || targetUnit.IsDead) return;
        if (!_barValues.ContainsKey(targetUnit))
            _barValues[targetUnit] = 0f;

        // 设为阈值+1
        _barValues[targetUnit] = barThreshold + 1f;
    }

    /// 开始一个单位的回合
    private void StartUnitTurn(CharacterStats unit)
    {
        _isActing = true;
        _currentUnit = unit;

        // 切换到移动阶段，广播事件
        ChangePhase(UnitPhase.Move);
        OnUnitTurnStart?.Invoke(unit);

        // 敌人自动执行AI
        if (unit.camp == CampType.Enemy)
        {
            EnemyAI enemy = unit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.StartEnemyTurn();
            }
            else
            {
                EndCurrentUnitTurn();
            }
        }
    }

    /// 统一切换回合阶段
    public void ChangePhase(UnitPhase newPhase)
    {
        // 切换阶段前统一重置格子高亮
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ResetAllTileColor();
        }

        CurrentPhase = newPhase;
        OnUnitPhaseChanged?.Invoke(_currentUnit, newPhase);
    }

    public void EndCurrentUnitTurn()
    {
        OnUnitTurnEnd?.Invoke(_currentUnit);

        _currentUnit = null;
        CurrentPhase = UnitPhase.Idle;
        _isActing = false;

        // 立刻推进行动条，开启下一个单位的回合
        AdvanceActionBar();
    }

    /// 单位死亡时从列表移除
    public void RemoveDeadUnit(CharacterStats deadUnit)
    {
        allUnits.Remove(deadUnit);
        if (_barValues.ContainsKey(deadUnit))
            _barValues.Remove(deadUnit);

        // 如果死亡的是当前行动单位，直接结束回合
        if (_currentUnit == deadUnit)
        {
            EndCurrentUnitTurn();
        }
    }

    /// 获取当前行动条排序（给UI用）
    public List<(CharacterStats unit, float value)> GetActionBarSorted()
    {
        List<(CharacterStats, float)> list = new();
        foreach (var kvp in _barValues)
        {
            if (!kvp.Key.IsDead)
                list.Add((kvp.Key, kvp.Value));
        }
        list.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        return list;
    }
}