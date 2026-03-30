using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float moveSpeed = 50f;
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if(Input.GetMouseButton(1)) 
        {
            Vector3 moveDir = mouseX * -transform.right + mouseY * -transform.up;
            moveDir.y = 0;
            transform.position += moveDir * Time.deltaTime * moveSpeed;
        }
    }
}
