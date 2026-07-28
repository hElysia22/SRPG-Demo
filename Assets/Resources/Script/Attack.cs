using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Attack : MonoBehaviour
{
    [Header("攻击设置")]
    public Animator animator;
    public LayerMask enemyLayer;

    private CharacterStats stats;
    private bool canAttack;
    private bool isAttacking;

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
        if (!canAttack || isAttacking) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                
                if (((1 << hit.collider.gameObject.layer) & enemyLayer) == 0)
                    return;

                int hitX = Mathf.RoundToInt(hit.point.x);
                int hitY = Mathf.RoundToInt(hit.point.z);
                GridData tile = GridManager.Instance.GetTile(hitX, hitY);

                if (tile == null || !GridManager.Instance.IsHighlight(tile))
                    return;

                StartCoroutine(AttackRoutine(hit.collider.gameObject));
            }
        }
    }

    private void OnUnitPhaseChanged(CharacterStats unit, UnitPhase phase)
    {
        if (unit != stats) return;

        if (phase == UnitPhase.Attack)
        {
            canAttack = true;
            CalculateAttackableGrid();
        }
        else
        {
            canAttack = false;
        }
    }

    private void OnUnitTurnEnd(CharacterStats unit)
    {
        if (unit != stats) return;
        canAttack = false;
        isAttacking = false;
    }

    IEnumerator AttackRoutine(GameObject target)
    {
        isAttacking = true;
        canAttack = false;

        // 面向目标
        Vector3 dir = target.transform.position - transform.position;
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

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // 造成伤害
        var targetStats = target.GetComponentInParent<CharacterStats>();
        if (targetStats != null)
        {
            int damage = stats != null ? stats.attack : 10;
            targetStats.TakeDamage(damage);
        }

        isAttacking = false;
        // 攻击结束，结束当前单位回合
        GameManage.Instance.EndCurrentUnitTurn();
    }

    public void CalculateAttackableGrid()
    {
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.z);
        int range = stats != null ? stats.attackRange : 2;
        GridData[] tiles = GridRangeHelper.CalculateAttackRange(x, y, range);
        GridManager.Instance.HighlightMoveableTiles(tiles);
        Debug.Log($"[{gameObject.name}] 计算攻击格子数：{tiles.Length}");
    }
}