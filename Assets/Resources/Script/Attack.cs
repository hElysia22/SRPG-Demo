using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public Animator animator;

    private GridData[] attackableTiles;

    [Header("攻击力")]
    public float attack;
    [Header("攻击范围")]
    public int attackRanage;
    [Header("当前坐标")]
    public int currentX;
    public int currentY;

    public bool canAttack = false;
    public bool isAttacking = false;

    public void UpdateCurrentPosition()
    {
        currentX = Mathf.RoundToInt(transform.position.x);
        currentY = Mathf.RoundToInt(transform.position.z);
    }

    void Start()
    {
        animator = transform.GetComponent<Animator>();
        if(animator == null ) 
        {
            Debug.Log("未找到动画");
            return;
        }
    }

    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack && !isAttacking)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 end = new Vector3(hit.transform.position.x, 0, hit.transform.position.z);
                int endX = Mathf.RoundToInt(end.x);
                int endY = Mathf.RoundToInt(end.z);
                GridData endTile = GridManager.Instance.GetTile(endX, endY);
                if (hit.collider.gameObject.layer == 6 && GridManager.Instance.isHighLight(endTile))
                {
                    StartCoroutine(PlayerAttackRoutine(hit.collider.gameObject));
                }
                else
                {
                    Debug.Log("未选中敌人或超出攻击范围");
                    return;
                }
            }
            
        }
    }

    IEnumerator PlayerAttackRoutine(GameObject enemyObject)
    {
        isAttacking = true;
        GameManage.Instance.EndAttackTurn();
        //面向敌人
        Vector3 dir = enemyObject.transform.position - transform.position;
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
        //执行攻击动画
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        //调用掉血代码
        Blood blood = enemyObject.GetComponentInParent<Blood>();
        if (blood != null)
        {
            blood.ReduceHp(attack);
        }

        //结束攻击
        isAttacking = false;
        GameManage.Instance.EndTurn();
    }

    public void CalculateAttackableGrid()
    {
        UpdateCurrentPosition();
        List<GridData> Tiles = new List<GridData>();
        HashSet<GridData> visited = new HashSet<GridData>();
        Queue<(int x, int y, int reminCost)> queue = new Queue<(int x, int y, int reminCost)>();

        GridData startTile = GridManager.Instance.GetTile(currentX, currentY);
        queue.Enqueue((currentX, currentY, attackRanage));
        visited.Add(startTile);
        (int x, int y)[] dirs = { (0, 1), (1, 0), (-1, 0), (0, -1) };


        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int x = current.x;
            int y = current.y;
            int remin = current.reminCost;

            if (remin <= 0)
            {
                continue;
            }

            foreach (var dir in dirs)
            {
                int newX = x + dir.x;
                int newY = y + dir.y;
                GridData neighborTile = GridManager.Instance.GetTile(newX, newY);

                if (neighborTile == null || visited.Contains(neighborTile) || !neighborTile.canWalk)
                {
                    continue;
                }

                visited.Add(neighborTile);
                Tiles.Add(neighborTile);
                queue.Enqueue((newX, newY, remin - 1));
            }
        }
        attackableTiles = Tiles.ToArray();
        GridManager.Instance.HighlightMoveableTiles(attackableTiles);
    }
}
