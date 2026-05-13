using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    public GameObject obj;

    private void LateUpdate()
    {
        Quaternion q = Quaternion.Euler(90,obj.transform.rotation.eulerAngles.y,0);
        transform.rotation = q;
    }
}
