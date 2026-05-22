using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

public class Blood : MonoBehaviour
{
    public Image BloodFilled;
    public float curHp = 100;        // 当前实际血量
    public float maxHp = 100;       // 最大血量
    public float smoothTime = 0.5f; // 渐变速度，越大越快
    private Coroutine _bloodChangedCoroutine;

    private float CalculateDamage(float damage)
    {
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currenty = Mathf.RoundToInt(transform.position.z);
        GridData currentGrid = GridManager.Instance.GetTile(currentX, currenty);
        float defend = currentGrid.defBonus;

        return Mathf.Max(0, damage - defend);
    }

   public void ReduceHp(float Hp)
   {
        float tarHp = curHp - CalculateDamage(Hp);
        tarHp = Mathf.Clamp(tarHp, 0, maxHp);
        if(_bloodChangedCoroutine != null)
        {
            StopCoroutine( _bloodChangedCoroutine );
        }
        _bloodChangedCoroutine = StartCoroutine(HpLerpCoroutine(curHp, tarHp));
        if(tarHp == 0 )
        {
            Die();
        }
   }

    private IEnumerator HpLerpCoroutine(float fromHp, float toHp)
    {
        float timer = 0f;
        while(timer < smoothTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / smoothTime);
            curHp = Mathf.Lerp(fromHp, toHp, t);
            BloodFilled.fillAmount = curHp / maxHp;
            yield return null;
        }
        curHp = toHp;
        BloodFilled.fillAmount = curHp / maxHp;
    }

    public void Die()
    {
        Destroy( transform.gameObject );
    }

    private void OnDestroy()
    {
        
    }
}
