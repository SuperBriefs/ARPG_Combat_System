using UnityEngine;
using UnityEngine.Animations;

public enum AICombatStates{ Idle, Chase, Circling }

public class CombatMovementState : State<EnemyController>
{
    [SerializeField] private float distanceToStand = 3f;
    [SerializeField] private float adjustDistanceThreashold = 1f;

    private AICombatStates state;

    EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;

        enemy.NavAgent.stoppingDistance = distanceToStand;
    }

    public override void Execute()
    {
        // +adjustDistanceThreashold 是为了预留一些空间 因为玩家一超过distanceToStand距离，敌人直接就追击是不自然的
        if(Vector3.Distance(enemy.Target.transform.position,  enemy.transform.position) > distanceToStand + adjustDistanceThreashold)
        {
            StartChase();
        }

        if(state == AICombatStates.Idle)
        {
            
        }
        else if (state == AICombatStates.Chase)
        {
            // +0.03f 是因为stoppingDistance的距离不是百分百准确的，需要来一点容错
            if(Vector3.Distance(enemy.Target.transform.position,  enemy.transform.position) <= distanceToStand + 0.03f)
            {
                StartIdle();
                return;
            }

            enemy.NavAgent.SetDestination(enemy.Target.transform.position);
        }
        else if (state == AICombatStates.Circling)
        {
            
        }
    }

    /// <summary>
    /// 开始等待
    /// </summary>
    private void StartIdle()
    {
        state = AICombatStates.Idle;
        enemy.Animator.SetBool("combatMode", true);
    }

    /// <summary>
    /// 开始追击
    /// </summary>
    private void StartChase()
    {
        state = AICombatStates.Chase;
        enemy.Animator.SetBool("combatMode", false);
    }

    public override void Exit()
    {
        
    }
}
