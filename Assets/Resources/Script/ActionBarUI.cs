using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionBarUI : MonoBehaviour
{
    [Header("配置")]
    public int showCount = 5; // 显示前几个
    public GameObject unitIconPrefab;
    public Transform iconParent;

    private List<GameObject> _icons = new();

    private void Update()
    {
        if (GameManage.Instance == null) return;
        RefreshBar();
    }

    private void RefreshBar()
    {
        var sorted = GameManage.Instance.GetActionBarSorted();

        // 生成/复用图标
        int count = Mathf.Min(sorted.Count, showCount);
        for (int i = 0; i < count; i++)
        {
            GameObject icon;
            if (i < _icons.Count)
            {
                icon = _icons[i];
            }
            else
            {
                icon = Instantiate(unitIconPrefab, iconParent);
                _icons.Add(icon);
            }

            icon.SetActive(true);
            // 这里可以设置头像、名字、进度条
            // 示例：用名字文本显示
            var txt = icon.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = sorted[i].unit.name;

            // 进度条填充
            var img = icon.transform.Find("Fill").GetComponent<Image>();
            if (img != null)
                img.fillAmount = Mathf.Clamp01(sorted[i].value / GameManage.Instance.barThreshold);
        }

        // 隐藏多余的
        for (int i = count; i < _icons.Count; i++)
        {
            _icons[i].SetActive(false);
        }
    }
}