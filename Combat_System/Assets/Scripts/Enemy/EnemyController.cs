using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { Idle, CombatMovement, Attack, RetreatAfterAttack, Dead, GettingHit }

public class EnemyController : MonoBehaviour
{
    [field: SerializeField] public float Fov { get; private set; } = 180;

    [field: SerializeField] public float AlertRange{ get; private set; } = 20f;

    public List<MeeleFighter> TargetsInRange { get; set; } = new List<MeeleFighter>();
    public MeeleFighter Target { get; set; }
    public float CombatMovementTimer { get; set; } = 0;

    public StateMachine<EnemyController> StateMachine { get; private set; }

    public Dictionary<EnemyStates, State<EnemyController>> stateDic;

    public NavMeshAgent NavAgent { get; private set; } 
    public CharacterController CharacterController { get; private set; } 
    public Animator Animator { get; private set; } 
    public MeeleFighter Fighter { get; private set; } 
    public SkinnedMeshHighlighter MeshHighlighter { get; private set; } 
    public VisionSensor VisionSensor { get; set; } 

    void Start()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        CharacterController = GetComponent<CharacterController>();
        Animator = GetComponent<Animator>();
        Fighter = GetComponent<MeeleFighter>();
        MeshHighlighter = GetComponent<SkinnedMeshHighlighter>();

        //初始化所有状态 避免每一次都要GetComponent
        stateDic = new Dictionary<EnemyStates, State<EnemyController>>();
        stateDic[EnemyStates.Idle] = GetComponent<IdleState>();
        stateDic[EnemyStates.CombatMovement] = GetComponent<CombatMovementState>();
        stateDic[EnemyStates.Attack] = GetComponent<EnemyAttackState>();
        stateDic[EnemyStates.RetreatAfterAttack] = GetComponent<RetreatAfterAttackState>();
        stateDic[EnemyStates.Dead] = GetComponent<DeadState>();
        stateDic[EnemyStates.GettingHit] = GetComponent<GettingHitState>();

        StateMachine = new StateMachine<EnemyController>(this);
        StateMachine.ChangeState(stateDic[EnemyStates.Idle]);

        Fighter.OnGoHit += (MeeleFighter attacker) => 
        {
            if(Fighter.Health > 0)
            {
                //受击后设置目标并警示周围敌人 避免击退状态结束后进入战斗运动状态没有目标
                if(Target == null)
                {
                    Target = attacker;
                    AlertNearbyEnemies();
                }

                ChangeState(EnemyStates.GettingHit);
            }
            else
            {
                ChangeState(EnemyStates.Dead);
            }
        };
    }

    /// <summary>
    /// 将状态转换再封装一层 方便调用
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(EnemyStates state)
    {
        StateMachine.ChangeState(stateDic[state]);
    }

    /// <summary>
    /// 判断当前敌人是否处于这个状态
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool IsInState(EnemyStates state)
    {
        return StateMachine.CurrentState == stateDic[state];
    }

    private Vector3 prevPos;
    void Update()
    {
        StateMachine.Execute();

        //计算每一帧的速度 主要是在Circling状态下，得到下一帧的移动位置是否会有障碍
        var deltaPos = Animator.applyRootMotion ? Vector3.zero : transform.position - prevPos; //有根运动的情况下，不进行移动，避免动画会有抖动
        var velocity = deltaPos / Time.deltaTime;

        // |v| * cosθ 敌人面朝向的速度
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);      
        Animator.SetFloat("forwardSpeed", forwardSpeed / NavAgent.speed, 0.2f, Time.deltaTime);

        float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
        //侧移速度 这里只要-1 ~ 1的数据即可
        float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
        Animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);

        //玩家死亡 敌人不再追击 敌人也不再是玩家可攻击的对象
        if(Target?.Health <= 0)
        {
            TargetsInRange.Remove(Target);
            EnemyManager.Instance.RemoveEnemyInRange(this);
        }

        prevPos = transform.position;
    }

    /// <summary>
    /// 获得范围内可以攻击的目标
    /// </summary>
    /// <returns></returns>
    public MeeleFighter FindTarget()
    {
        //遍历范围内所有的可追目标 选取在视角内容的目标追击
        foreach(var target in TargetsInRange)
        {
            var vecToTarget = target.transform.position - transform.position;
            var angle = Vector3.Angle(transform.forward, vecToTarget);

            if(angle <= Fov / 2)
            {
                return target;
            }
        }

        return null;
    }

    /// <summary>
    /// 警示临近敌人
    /// </summary>
    public void AlertNearbyEnemies()
    {
        var colliders = Physics.OverlapBox(transform.position, new Vector3(AlertRange / 2f, 1, AlertRange / 2f), 
                           Quaternion.identity, EnemyManager.Instance.EnemyLayer);

        foreach(var collider in colliders)
        {
            //忽略当前正在警示的敌人
            if(collider.gameObject == gameObject) continue;

            var nearbyEnemy = collider.GetComponent<EnemyController>();
            //警示周围在等待状态的敌人
            if(nearbyEnemy != null && nearbyEnemy.Target == null && nearbyEnemy.IsInState(EnemyStates.Idle))
            {
                //让临近的敌人一起追击玩家
                nearbyEnemy.Target = Target;
                nearbyEnemy.ChangeState(EnemyStates.CombatMovement);
            }
        }
    }
}
