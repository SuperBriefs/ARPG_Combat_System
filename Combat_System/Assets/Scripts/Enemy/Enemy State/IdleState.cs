using UnityEngine;

public class IdleState : State<EnemyController>
{
    private EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;
    }

    public override void Execute()
    {
        //遍历范围内所有的可追目标 选取在视角内容的目标追击
        foreach(var target in enemy.TargetsInRange)
        {
            var vecToTarget = target.transform.position - transform.position;
            var angle = Vector3.Angle(transform.forward, vecToTarget);

            if(angle <= enemy.Fov / 2)
            {
                enemy.Target = target;
                enemy.ChangeState(EnemyStates.CombatMovementState);
                break;
            }
        }
    }

    public override void Exit()
    {
        
    }
}
