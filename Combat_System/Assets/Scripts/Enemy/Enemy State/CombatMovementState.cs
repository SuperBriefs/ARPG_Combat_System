using UnityEngine;
using UnityEngine.Animations;

public enum AICombatStates{ Idle, Chase, Circling }

public class CombatMovementState : State<EnemyController>
{
    [SerializeField] private float circlingSpeed = 20f;
    [SerializeField] private float distanceToStand = 3f;
    [SerializeField] private float adjustDistanceThreashold = 1f;
    [SerializeField] private Vector2 idleTimeRange = new Vector2(2, 5);
    [SerializeField] private Vector2 circlingTimeRange = new Vector2(3, 6);

    private float timer = 0;
    private int circlingDir = 1; //1向左盘旋 -1向右盘旋

    private AICombatStates state;

    EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;

        enemy.NavAgent.stoppingDistance = distanceToStand;
        enemy.CombatMovementTimer = 0;
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
            //当计时器结束，50%概率继续等待，50%概率绕着玩家盘旋
            if(timer <= 0)
            {
                if(Random.Range(0, 2) == 0)
                {
                    StartIdle();
                }
                else
                {
                    StartCircling();
                }
            }
        }
        else if (state == AICombatStates.Chase)
        {
            // +0.03f 是因为stoppingDistance的距离不是百分百准确的，需要来一点容错
            if(Vector3.Distance(enemy.Target.transform.position, enemy.transform.position) <= distanceToStand + 0.03f)
            {
                StartIdle();
                return;
            }

            enemy.NavAgent.SetDestination(enemy.Target.transform.position);
        }
        else if (state == AICombatStates.Circling)
        {
            if(timer <= 0)
            {
                StartIdle();
                return;
            }
            
            //transform.RotateAround(enemy.Target.transform.position, Vector3.up, circlingSpeed * circlingDir * Time.deltaTime);
        
            var vecToTarget = enemy.transform.position - enemy.Target.transform.position;
            //在y轴上旋转后的结果
            var rotatedPos = Quaternion.Euler(0, circlingSpeed * circlingDir * Time.deltaTime, 0) * vecToTarget;

            //NavMeshAgent.Move是直接位移
            enemy.NavAgent.Move(rotatedPos - vecToTarget);

            //敌人始终面向玩家
            enemy.transform.rotation = Quaternion.LookRotation(-rotatedPos);
        }

        //计时器变化
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }

        //敌人进入战斗运动状态的时间
        enemy.CombatMovementTimer += Time.deltaTime;
    }

    /// <summary>
    /// 开始等待
    /// </summary>
    private void StartIdle()
    {
        state = AICombatStates.Idle;
        timer = Random.Range(idleTimeRange.x, idleTimeRange.y);
        
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

    /// <summary>
    /// 开始在玩家周围盘旋
    /// </summary>
    private void StartCircling()
    {
        state = AICombatStates.Circling;

        //清除敌人追踪玩家的路径 避免遇到障碍时，敌人无法正常盘旋
        enemy.NavAgent.ResetPath();

        timer = Random.Range(circlingTimeRange.x, circlingTimeRange.y);

        //50%向左盘旋 50%向右盘旋
        circlingDir = Random.Range(0, 2) == 0 ? 1 : -1;
    }

    public override void Exit()
    {
        enemy.CombatMovementTimer = 0;
    }
}
