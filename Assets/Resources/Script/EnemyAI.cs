using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Animator enemyAnim;
    public float moveSpeed = 3f;

    private GameObject targetPlayer;
    private GridData[] moveableTiles;
    private CharacterStats stats;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        if (enemyAnim == null)
            enemyAnim = GetComponent<Animator>();
    }

    /// 查找距离最近的存活玩家单位
    private GameObject FindNearestPlayer()
    {
        List<CharacterStats> allUnits = GameManage.Instance.allUnits;
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var unit in allUnits)
        {
            // 只找玩家阵营、且存活的单位
            if (unit.camp != CampType.Player || unit.IsDead)
                continue;

            float distX = Mathf.Abs(transform.position.x - unit.transform.position.x);
            float distZ = Mathf.Abs(transform.position.z - unit.transform.position.z);
            float totalDist = distX + distZ;

            if (totalDist < minDist)
            {
                minDist = totalDist;
                nearest = unit.gameObject;
            }
        }

        return nearest;
    }

    public void StartEnemyTurn()
    {
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        // 切换到移动阶段，触发全局事件
        GameManage.Instance.ChangePhase(UnitPhase.Move);

        yield return new WaitForSeconds(0.4f);

        targetPlayer = FindNearestPlayer();
        // 没有可攻击目标，直接结束回合
        if (targetPlayer == null)
        {
            GameManage.Instance.EndCurrentUnitTurn();
            yield break;
        }

        int curX = Mathf.RoundToInt(transform.position.x);
        int curY = Mathf.RoundToInt(transform.position.z);
        int moveCost = stats != null ? stats.moveCost : 3;

        moveableTiles = GridRangeHelper.CalculateMoveRange(curX, curY, moveCost);

        // 寻找离玩家最近的可移动格子
        if (moveableTiles.Length > 0)
        {
            int playerX = Mathf.RoundToInt(targetPlayer.transform.position.x);
            int playerY = Mathf.RoundToInt(targetPlayer.transform.position.z);

            GridData bestTile = moveableTiles[0];
            float minDist = float.MaxValue;

            foreach (var tile in moveableTiles)
            {
                float dist = Mathf.Abs(tile.x - playerX) + Mathf.Abs(tile.y - playerY);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestTile = tile;
                }
            }

            Vector3 endPos = new Vector3(bestTile.x, 0, bestTile.y);
            // 先释放当前占用的格子
            GridManager.Instance.ResetMove(curX, curY);

            List<Vector3> path = AStar.FindPath(transform.position, endPos);

            if (path.Count > 0)
            {
                yield return StartCoroutine(EnemyMoveCoroutine(path));
            }
            else
            {
                // 寻路失败，重新占用原格子
                GridManager.Instance.SetMoveFalse(curX, curY);
            }
        }

        yield return new WaitForSeconds(0.2f);

        // 切换到攻击阶段，触发全局事件
        GameManage.Instance.ChangePhase(UnitPhase.Attack);

        // 攻击前二次校验：目标是否还存活
        if (targetPlayer != null)
        {
            CharacterStats targetStats = targetPlayer.GetComponent<CharacterStats>();
            if (targetStats != null && !targetStats.IsDead && IsPlayerInAttackRange(targetPlayer))
            {
                // 面向玩家
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

                // 攻击动画
                enemyAnim.SetTrigger("Attack");
                yield return new WaitForSeconds(enemyAnim.GetCurrentAnimatorStateInfo(0).length);

                // 造成伤害
                if (targetStats != null)
                {
                    int damage = stats != null ? stats.attack : 10;
                    targetStats.TakeDamage(damage);
                }
            }
        }

        // 结束当前单位回合
        GameManage.Instance.EndCurrentUnitTurn();
    }

    /// 判断目标是否在攻击范围内
    private bool IsPlayerInAttackRange(GameObject player)
    {
        int ex = Mathf.RoundToInt(transform.position.x);
        int ey = Mathf.RoundToInt(transform.position.z);
        int px = Mathf.RoundToInt(player.transform.position.x);
        int py = Mathf.RoundToInt(player.transform.position.z);

        int range = stats != null ? stats.attackRange : 2;
        return GridRangeHelper.IsInManhattanRange(ex, ey, px, py, range);
    }

    /// 敌人移动协程
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

        // 移动完成，占用新格子
        int endX = Mathf.RoundToInt(transform.position.x);
        int endY = Mathf.RoundToInt(transform.position.z);
        GridManager.Instance.SetMoveFalse(endX, endY);
    }
}