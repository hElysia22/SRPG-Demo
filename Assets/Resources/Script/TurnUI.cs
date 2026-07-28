using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    [Header("回合操作按钮")]
    public Button endMoveBtn;    // 结束移动按钮
    public Button endAttackBtn;  // 结束攻击按钮
    public Button itemBtn;       // 道具按钮

    [Header("道具箱配置")]
    public RectTransform itemBox;      // 道具箱背景
    public GameObject itemContent;     // 道具内容父物体
    public float maxWidth = 100f;
    public float SlideTime = 0.3f;

    private bool isOpen = false;
    private bool isSlide = false;

    private void Awake()
    {
        // 初始化隐藏所有操作按钮
        endMoveBtn.gameObject.SetActive(false);
        endAttackBtn.gameObject.SetActive(false);
        itemBtn.gameObject.SetActive(false);
    }

    private void Start()
    {
        // 道具箱初始状态
        itemBox.sizeDelta = new Vector2(0, itemBox.sizeDelta.y);
        itemContent.SetActive(false);

        if (GameManage.Instance != null)
        {
            GameManage.Instance.OnUnitPhaseChanged += OnUnitPhaseChanged;
            GameManage.Instance.OnUnitTurnEnd += OnUnitTurnEnd;
            Debug.Log("TurnUI：事件订阅成功");
        }
        else
        {
            Debug.LogError("TurnUI：找不到GameManage.Instance，请检查场景里是否有GameManage脚本");
        }
    }


    private void OnDisable()
    {
        if (GameManage.Instance == null) return;
        GameManage.Instance.OnUnitPhaseChanged -= OnUnitPhaseChanged;
        GameManage.Instance.OnUnitTurnEnd -= OnUnitTurnEnd;
    }

    #region 回合阶段UI响应
    /// 阶段切换时自动更新按钮显隐
    private void OnUnitPhaseChanged(CharacterStats unit, UnitPhase phase)
    {
        // 敌人回合：隐藏所有玩家操作按钮，关闭道具箱
        if (unit.camp == CampType.Enemy)
        {
            SetAllButtonsActive(false);
            if (isOpen) CloseBoxImmediately();
            return;
        }

        // 玩家回合：根据阶段显示对应按钮
        switch (phase)
        {
            case UnitPhase.Move:
                endMoveBtn.gameObject.SetActive(true);
                itemBtn.gameObject.SetActive(true);
                endAttackBtn.gameObject.SetActive(false);
                break;
            case UnitPhase.Attack:
                endAttackBtn.gameObject.SetActive(true);
                endMoveBtn.gameObject.SetActive(false);
                itemBtn.gameObject.SetActive(false);
                // 进入攻击阶段自动关闭道具箱
                if (isOpen) CloseBoxImmediately();
                break;
            default:
                SetAllButtonsActive(false);
                break;
        }
    }

    /// 单位回合结束时隐藏所有按钮
    private void OnUnitTurnEnd(CharacterStats unit)
    {
        SetAllButtonsActive(false);
        if (isOpen) CloseBoxImmediately();
    }

    /// 批量设置所有操作按钮显隐
    private void SetAllButtonsActive(bool active)
    {
        endMoveBtn.gameObject.SetActive(active);
        endAttackBtn.gameObject.SetActive(active);
        itemBtn.gameObject.SetActive(active);
    }
    #endregion

    #region 按钮点击事件（Inspector绑定OnClick）
    /// 结束移动按钮点击：切换到攻击阶段
    public void OnEndMoveClicked()
    {
        GameManage.Instance.ChangePhase(UnitPhase.Attack);
    }

    /// 结束攻击按钮点击：结束当前单位回合
    public void OnEndAttackClicked()
    {
        GameManage.Instance.EndCurrentUnitTurn();
    }

    /// 道具按钮点击：开关道具箱
    public void OnItemBtnClicked()
    {
        ToggleBox();
    }
    #endregion

    #region 原有道具箱滑动逻辑（完全保留）
    public void ToggleBox()
    {
        if (isSlide) return;

        if (isOpen)
        {
            StartCoroutine(CloseBox());
        }
        else
        {
            StartCoroutine(OpenBox());
        }
    }

    IEnumerator OpenBox()
    {
        isSlide = true;
        float timer = 0;

        while (timer < SlideTime)
        {
            timer += Time.deltaTime;
            float width = Mathf.Lerp(0, maxWidth, timer / SlideTime);
            itemBox.sizeDelta = new Vector2(width, itemBox.sizeDelta.y);
            yield return null;
        }
        itemBox.sizeDelta = new Vector2(maxWidth, itemBox.sizeDelta.y);
        itemContent.SetActive(true);

        isOpen = true;
        isSlide = false;
    }

    IEnumerator CloseBox()
    {
        isSlide = true;
        itemContent.SetActive(false);

        float timer = 0;
        while (timer < SlideTime)
        {
            timer += Time.deltaTime;
            float width = Mathf.Lerp(maxWidth, 0, timer / SlideTime);
            itemBox.sizeDelta = new Vector2(width, itemBox.sizeDelta.y);
            yield return null;
        }

        itemBox.sizeDelta = new Vector2(0, itemBox.sizeDelta.y);

        isOpen = false;
        isSlide = false;
    }

    /// 立即关闭道具箱（无动画）
    private void CloseBoxImmediately()
    {
        StopAllCoroutines();
        itemContent.SetActive(false);
        itemBox.sizeDelta = new Vector2(0, itemBox.sizeDelta.y);
        isOpen = false;
        isSlide = false;
    }
    #endregion
}