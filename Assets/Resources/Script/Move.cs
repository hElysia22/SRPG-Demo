using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Move : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    public Animator animator;

    private CharacterStats stats;
    private int currentX;
    private int currentY;
    private bool isMoving;
    private bool canMove;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (GameManage.Instance != null)
        {
            GameManage.Instance.OnUnitPhaseChanged += OnUnitPhaseChanged;
            GameManage.Instance.OnUnitTurnEnd += OnUnitTurnEnd;
        }
    }

    private void Start()
    {
        UpdateCurrentPosition();
        if (GameManage.Instance != null && GameManage.Instance.CurrentUnit == stats)
        {
            OnUnitPhaseChanged(stats, GameManage.Instance.CurrentPhase);
        }
    }

    private void OnDestroy()
    {
        if (GameManage.Instance != null)
        {
            GameManage.Instance.OnUnitPhaseChanged -= OnUnitPhaseChanged;
            GameManage.Instance.OnUnitTurnEnd -= OnUnitTurnEnd;
        }
    }

    private void Update()
    {
        if (!canMove || isMoving) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                int endX = Mathf.RoundToInt(hit.point.x);
                int endY = Mathf.RoundToInt(hit.point.z);
                GridData endTile = GridManager.Instance.GetTile(endX, endY);

                if (endTile == null || !GridManager.Instance.IsHighlight(endTile))
                    return;

                // 开始移动：释放原格子
                GridManager.Instance.ResetMove(currentX, currentY);
                List<Vector3> path = AStar.FindPath(transform.position, hit.point);

                if (path.Count > 0)
                {
                    canMove = false;
                    StartCoroutine(MoveByGrid(path));
                }
                else
                {
                    // 寻路失败，重新占用原格子
                    GridManager.Instance.SetMoveFalse(currentX, currentY);
                }
            }
        }
    }

    // 阶段变化响应
    private void OnUnitPhaseChanged(CharacterStats unit, UnitPhase phase)
    {
        if (unit != stats) return;

        if (phase == UnitPhase.Move)
        {
            canMove = true;
            CalculateMoveableGrid();
        }
        else
        {
            canMove = false;
        }
    }

    // 回合结束响应
    private void OnUnitTurnEnd(CharacterStats unit)
    {
        if (unit != stats) return;
        canMove = false;
        isMoving = false;
    }

    // 计算可移动范围
    public void CalculateMoveableGrid()
    {
        UpdateCurrentPosition();
        int cost = stats != null ? stats.moveCost : 3;
        GridData[] tiles = GridRangeHelper.CalculateMoveRange(currentX, currentY, cost);
        GridManager.Instance.HighlightMoveableTiles(tiles);
        Debug.Log($"[{gameObject.name}] 计算可移动格子数：{tiles.Length}");
    }

    // 更新当前坐标
    public void UpdateCurrentPosition()
    {
        currentX = Mathf.RoundToInt(transform.position.x);
        currentY = Mathf.RoundToInt(transform.position.z);
    }

    // 格子移动协程
    IEnumerator MoveByGrid(List<Vector3> pathList)
    {
        isMoving = true;
        animator.SetFloat("Speed", moveSpeed);

        foreach (var targetPos in pathList)
        {
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
            }

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
        }

        animator.SetFloat("Speed", 0);
        UpdateCurrentPosition();
        GridManager.Instance.SetMoveFalse(currentX, currentY);
        isMoving = false;

        // 移动结束，切换到攻击阶段
        GameManage.Instance.ChangePhase(UnitPhase.Attack);
    }
}