using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Move : MonoBehaviour
{
    public Animator animator;

    [Header("角色设置")]
    public float moveSpeed = 0.1f;
    public int moveCost = 6;
    
    [Header("当前坐标")]
    public int currentX;
    public int currentY;

    private bool isMoving = false;
    public bool canMove = false;
    private GridData[] moveableTiles;

    public void UpdateCurrentPosition()
    {
        currentX = Mathf.RoundToInt(transform.position.x);
        currentY = Mathf.RoundToInt(transform.position.z);
    }

    private void Start()
    {
        animator = transform.GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving && canMove)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 获取起点和终点
                Vector3 start = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 end = new Vector3(hit.transform.position.x, 0, hit.transform.position.z);
                int endX = Mathf.RoundToInt(end.x);
                int endY = Mathf.RoundToInt(end.z);
                UpdateCurrentPosition();
                GridData endTile = GridManager.Instance.GetTile(endX, endY);
                if(!GridManager.Instance.isHighLight(endTile))
                {
                    Debug.Log("超出可移动范围");
                    return;
                }
                if(endTile.canWalk == false)
                {
                    Debug.Log("无法移动到目标位置！");
                    return;
                }
                // 调用A*寻路
                GridManager.Instance.ResetMove(currentX, currentY);
                List<Vector3> path = AStar.FindPath(start, end);

                if (path.Count > 0)
                {
                    isMoving = true;
                    StartCoroutine(MoveByGrid(path));
                    GameManage.Instance.EndMoveTurn();
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
            dir.y = 0;
            if (dir.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
                    yield return null;
                }
                transform.rotation = targetRot;
            }
            
            // 移动
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                animator.SetFloat("Speed", moveSpeed);
                yield return null;
            }
            transform.position = targetPos;
        }
        animator.SetFloat("Speed", 0);
        isMoving = false;
        GameManage.Instance.StartAttackTurn();
        UpdateCurrentPosition();
        GridManager.Instance.SetMoveFalse(currentX, currentY);
    }

    public void CalculateMoveableGrid()
    {
        UpdateCurrentPosition();
        List<GridData> Tiles = new List<GridData>();
        HashSet<GridData> visited = new HashSet<GridData>();
        Queue<(int x, int y, int reminCost)> queue = new Queue<(int x, int y, int reminCost)>();

        GridData startTile = GridManager.Instance.GetTile(currentX, currentY);
        queue.Enqueue((currentX, currentY, moveCost));
        visited.Add(startTile);
        (int x, int y)[] dirs = { (0, 1), (1, 0), (-1, 0), (0, -1) };


        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int x = current.x;
            int y = current.y;
            int remin = current.reminCost;

            if(remin <= 0)
            {
                continue;
            }

            foreach ( var dir in dirs )
            {
                int newX = x + dir.x;
                int newY = y + dir.y;
                GridData neighborTile = GridManager.Instance.GetTile(newX, newY);

                if(neighborTile == null || visited.Contains(neighborTile) || !neighborTile.canWalk)
                {
                    continue;
                }

                visited.Add(neighborTile);
                Tiles.Add(neighborTile);
                queue.Enqueue((newX, newY, remin - 1));
            }
        }
        moveableTiles = Tiles.ToArray();
        GridManager.Instance.HighlightMoveableTiles(moveableTiles);
    }
}