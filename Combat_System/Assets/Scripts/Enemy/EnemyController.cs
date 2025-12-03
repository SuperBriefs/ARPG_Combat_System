using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { Idle, CombatMovementState }

public class EnemyController : MonoBehaviour
{
    [field: SerializeField] public float Fov { get; private set; } = 180;

    public List<MeeleFighter> TargetsInRange { get; set; } = new List<MeeleFighter>();
    public MeeleFighter Target { get; set; }

    public StateMachine<EnemyController> StateMachine { get; private set; }

    public Dictionary<EnemyStates, State<EnemyController>> stateDic;

    public NavMeshAgent NavAgent { get; private set; } 
    public Animator Animator { get; private set; } 

    void Start()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();

        //初始化所有状态 避免每一次都要GetComponent
        stateDic = new Dictionary<EnemyStates, State<EnemyController>>();
        stateDic[EnemyStates.Idle] = GetComponent<IdleState>();
        stateDic[EnemyStates.CombatMovementState] = GetComponent<CombatMovementState>();

        StateMachine = new StateMachine<EnemyController>(this);
        StateMachine.ChangeState(stateDic[EnemyStates.Idle]);
    }

    /// <summary>
    /// 将状态转换再封装一层 方便调用
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(EnemyStates state)
    {
        StateMachine.ChangeState(stateDic[state]);
    }

    void Update()
    {
        StateMachine.Execute();
        
        Animator.SetFloat("moveAmount", NavAgent.velocity.magnitude / NavAgent.speed);
    }
}
