using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public int moveCost = 3;
    public int attackRange = 2;
    public float attack = 20;
    public Animator enemyAnim;
    public float moveSpeed = 3f;

    GameObject targetPlayer;
    private GridData[] moveableTiles;

    private void Start()
    {
        enemyAnim = GetComponent<Animator>();
    }

    // 自动找到最近的玩家（从GameManage的列表里找）
    private GameObject FindNearestPlayer()
    {
        List<GameObject> players = GameManage.Instance.Players;
        GameObject nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var player in players)
        {
            
            if (player.GetComponent<EnemyAI>() != null)
                continue;

            float distX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distZ = Mathf.Abs(transform.position.z - player.transform.position.z);
            float totalDist = distX + distZ;
            if (totalDist < minDistance)
            {
                minDistance = totalDist;
                nearestPlayer = player;
            }
        }
        return nearestPlayer;
    }

    public void StartEnemyTurn()
    {
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(0.4f);

        // 自动获取最近玩家
        targetPlayer = FindNearestPlayer();

        int curX = Mathf.RoundToInt(transform.position.x);
        int curY = Mathf.RoundToInt(transform.position.z);
        moveableTiles = CalculateEnemyGrid(curX, curY, moveCost);

        if (moveableTiles != null && moveableTiles.Length > 0 && targetPlayer != null)
        {
            // 最近玩家坐标
            int playerX = Mathf.RoundToInt(targetPlayer.transform.position.x);
            int playerY = Mathf.RoundToInt(targetPlayer.transform.position.z);

            GridData bestTile = moveableTiles[0];
            float minDist = float.MaxValue;
            foreach (var t in moveableTiles)
            {
                float dist = Mathf.Abs(t.x - playerX) + Mathf.Abs(t.y - playerY);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestTile = t;
                }
            }

            Vector3 endPos = new Vector3(bestTile.x, 0, bestTile.y);
            List<Vector3> path = AStar.FindPath(transform.position, endPos);

            if (path != null && path.Count > 0)
            {
                yield return StartCoroutine(EnemyMoveCoroutine(path));
            }
        }

        yield return new WaitForSeconds(0.2f);
        //攻击逻辑
        if (targetPlayer != null)
        {
            // 检查玩家是否在攻击范围内
            Debug.Log(IsPlayerInAttackRange(targetPlayer));
            if (IsPlayerInAttackRange(targetPlayer))
            {
                //面向敌人
                Vector3 dir = targetPlayer.transform.position - transform.position;
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
                //攻击动画
                enemyAnim.SetTrigger("Attack");
                yield return new WaitForSeconds(enemyAnim.GetCurrentAnimatorStateInfo(0).length);

                //掉血逻辑
                targetPlayer.transform.GetComponent<Blood>().ReduceHp(attack);
                yield return null;
            }
        }
        
        // 结束回合
        GameManage.Instance.EndTurn();
    }

    // 判断玩家是否在攻击范围内
    private bool IsPlayerInAttackRange(GameObject player)
    {
        int ex = Mathf.RoundToInt(transform.position.x);
        int ey = Mathf.RoundToInt(transform.position.z);

        int px = Mathf.RoundToInt(player.transform.position.x);
        int py = Mathf.RoundToInt(player.transform.position.z);

        // 曼哈顿距离
        int distance = Mathf.Abs(ex - px) + Mathf.Abs(ey - py);

        return distance <= attackRange;
    }

    IEnumerator EnemyMoveCoroutine(List<Vector3> pathList)
    {
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
                transform.rotation = targetRot;
            }

            enemyAnim.SetFloat("Speed", moveSpeed);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
        }
        enemyAnim.SetFloat("Speed", 0);
    }

    private GridData[] CalculateEnemyGrid(int startX, int startY, int cost)
    {
        List<GridData> Tiles = new List<GridData>();
        HashSet<GridData> visited = new HashSet<GridData>();
        Queue<(int x, int y, int reminCost)> queue = new Queue<(int x, int y, int reminCost)>();

        GridData startTile = GridManager.Instance.GetTile(startX, startY);
        queue.Enqueue((startX, startY, cost));
        visited.Add(startTile);
        (int x, int y)[] dirs = { (0, 1), (1, 0), (-1, 0), (0, -1) };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int x = current.x;
            int y = current.y;
            int remin = current.reminCost;

            if (remin <= 0) continue;

            foreach (var dir in dirs)
            {
                int newX = x + dir.x;
                int newY = y + dir.y;
                GridData neighborTile = GridManager.Instance.GetTile(newX, newY);

                if (neighborTile == null || visited.Contains(neighborTile) || !neighborTile.canWalk)
                    continue;

                visited.Add(neighborTile);
                Tiles.Add(neighborTile);
                queue.Enqueue((newX, newY, remin - 1));
            }
        }
        return Tiles.ToArray();
    }
}