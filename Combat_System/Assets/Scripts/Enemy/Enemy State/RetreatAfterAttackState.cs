using UnityEngine;

public class RetreatAfterAttackState : State<EnemyController>
{
    [SerializeField] private float backwardWalkSpeed = 1.5f;
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float distanceToRetreat = 3f;

    private EnemyController enemy;
    Vector3 targetPos;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;
        targetPos = enemy.Target.transform.position;
    }

    public override void Execute()
    {
        //只在进入退后状态时 确定要相对哪个位置后退 避免玩家跟着敌人，敌人会一直后退
        if(Vector3.Distance(enemy.transform.position, targetPos) >= distanceToRetreat)
        {
            enemy.ChangeState(EnemyStates.CombatMovement);
            return;
        }

        var vecToTarget = enemy.Target.transform.position - enemy.transform.position;
        enemy.NavAgent.Move(-vecToTarget.normalized * backwardWalkSpeed * Time.deltaTime);

        //始终面向玩家
        vecToTarget.y = 0; //竖直方向上不旋转
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(vecToTarget), rotateSpeed * Time.deltaTime);
    }

    public override void Exit()
    {
        
    }
}
