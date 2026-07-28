using UnityEngine;

public class SpeedBuff : MonoBehaviour
{
    public int speedBonus = 50;
    public int duration = 3; // 持续3回合

    private CharacterStats _owner;
    private int _remainTurns;

    private void Awake()
    {
        _owner = GetComponent<CharacterStats>();
    }

    private void OnEnable()
    {
        _remainTurns = duration;
        _owner.speed += speedBonus;
        GameManage.Instance.OnUnitTurnEnd += OnTurnEnd;
    }

    private void OnDisable()
    {
        if (GameManage.Instance != null)
            GameManage.Instance.OnUnitTurnEnd -= OnTurnEnd;

        _owner.speed -= speedBonus;
    }

    private void OnTurnEnd(CharacterStats unit)
    {
        if (unit != _owner) return;

        _remainTurns--;
        if (_remainTurns <= 0)
        {
            Destroy(this);
        }
    }
}