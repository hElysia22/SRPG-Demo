using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool isMoving = false;
    public bool canPlay = false;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving && canPlay)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 获取起点和终点
                Vector3 start = new Vector3(transform.position.x, 1, transform.position.z);
                Vector3 end = new Vector3(hit.transform.position.x, 1, hit.transform.position.z);
                int startX = Mathf.RoundToInt(start.x);
                int startY = Mathf.RoundToInt(start.z);
                // 调用A*寻路
                GridManager.Instance.ResetMove(startX, startY);
                List<Vector3> path = AStar.FindPath(start, end);

                if (path.Count > 0)
                {
                    isMoving = true;
                    StartCoroutine(MoveByGrid(path));
                }
                else
                {
                    Debug.Log("无法移动到目标位置！");
                }
            }
        }
    }

    // 协程移动
    private IEnumerator MoveByGrid(List<Vector3> pathList)
    {
        foreach (var targetPos in pathList)
        {
            // 转向
            Vector3 dir = targetPos - transform.position;
            if (dir.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
                    yield return null;
                }
            }

            // 移动
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
        }

        isMoving = false;
        GameManage.Instance.EndTurn();
        int endX = Mathf.RoundToInt(transform.position.x);
        int endY = Mathf.RoundToInt(transform.position.z);
        GridManager.Instance.SetMoveFalse(endX, endY);
    }
}