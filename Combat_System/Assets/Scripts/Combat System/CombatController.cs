using UnityEngine;

public class CombatController : MonoBehaviour
{
    private EnemyController targetEnemy;
    public EnemyController TargetEnemy
    {
        get => targetEnemy;
        set
        {
            targetEnemy = value;

            //没有目标敌人就始终无法进入战斗模式
            if(targetEnemy == null)
            {
                combatMode = false;
            }
        }
    }

    private bool combatMode;
    public bool CombatMode
    {
        get => combatMode;
        set
        {
            combatMode = value;

            //没有目标敌人就始终无法进入战斗模式
            if(TargetEnemy == null)
                combatMode = false;

            animator.SetBool("combatMode", combatMode);
        }
    }

    private MeeleFighter meeleFighter;
    private Animator animator;
    private CameraController cam;

    void Awake()
    {
        meeleFighter = GetComponent<MeeleFighter>();
        animator = GetComponent<Animator>();
        cam = Camera.main.GetComponent<CameraController>();
    }

    void Start()
    {
        //玩家受击后 切换目标敌人
        meeleFighter.OnGoHit += (MeeleFighter attacker) =>
        {
            //锁敌状态下才可以 注意高光变化
            if(CombatMode && attacker != TargetEnemy.Fighter)
            {
                TargetEnemy.MeshHighlighter?.HighlightMesh(false);
                TargetEnemy = attacker.GetComponent<EnemyController>();
                TargetEnemy.MeshHighlighter?.HighlightMesh(true);
            }
        };
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !meeleFighter.IsTakingHit /*不在受击状态下才可以攻击*/)
        {
            var enemy = EnemyManager.Instance.GetAttackingEnemy();
            if(enemy != null && enemy.Fighter.IsCounterable && !meeleFighter.InAction && meeleFighter.InVisionToCounter(enemy))
            {
                StartCoroutine(meeleFighter.PerformCounterAttack(enemy));
            }
            else
            {
                //攻击输入方向上最近的敌人 玩家受击时，找的最近敌人方向是受击后的面朝向
                var enemyToAttack = EnemyManager.Instance.GetClosesEnemyToDirection(PlayerController.Instance.GetIntentDirection());

                meeleFighter.TryToAttack(enemyToAttack?.Fighter);
                //攻击敌人也将锁敌
                //CombatMode = true;
            }
        }

        //按F键进行锁敌
        if (Input.GetKeyDown(KeyCode.F))
        {
            CombatMode = !CombatMode;
        }
    }

    /// <summary>
    /// 当Animator使用Root Motion时，每一帧都会调用这个方法，这样就可以手动控制动画驱动的位移和旋转
    /// </summary>
    void OnAnimatorMove()
    {
        //玩家反击的时候 不使用根驱动位移
        if(!meeleFighter.InCounter)
        {
            transform.position += animator.deltaPosition;
        }
        transform.rotation *= animator.deltaRotation;
    }

    /// <summary>
    /// 得到玩家视线方向 就是相机与玩家之间向量的水平分量
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTargetingDir()
    {
        //没有锁敌的情况下 才会找玩家视线方向来锁定最近的敌人
        if(!CombatMode)
        {
            var vecFromCam = transform.position - cam.transform.position;
            vecFromCam.y = 0;
            return vecFromCam.normalized;
        }
        //有锁敌的情况下 用玩家面朝向来锁敌
        else
        {
            return transform.forward;
        }
    }
}
