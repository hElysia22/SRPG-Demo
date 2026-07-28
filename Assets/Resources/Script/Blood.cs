using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Blood : MonoBehaviour
{
    public Image bloodFilled;
    public float smoothTime = 0.5f;

    private Coroutine bloodCoroutine;

    /// 外部调用更新血条
    public void UpdateHp(int currentHp, int maxHp)
    {
        float targetFill = (float)currentHp / maxHp;

        if (bloodCoroutine != null)
            StopCoroutine(bloodCoroutine);

        bloodCoroutine = StartCoroutine(HpLerpCoroutine(bloodFilled.fillAmount, targetFill));
    }

    IEnumerator HpLerpCoroutine(float from, float to)
    {
        float timer = 0f;
        while (timer < smoothTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / smoothTime);
            bloodFilled.fillAmount = Mathf.Lerp(from, to, t);
            yield return null;
        }
        bloodFilled.fillAmount = to;
    }
}