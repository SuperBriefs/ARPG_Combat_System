using UnityEngine;

public class IdleState : State<EnemyController>
{
    private EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;
        
        enemy.Animator.SetBool("combatMode", false);
    }

    public override void Execute()
    {
        //选取在视角内最近的目标追击
        enemy.Target = enemy.FindTarget();
        if(enemy.Target != null)
        {
            enemy.ChangeState(EnemyStates.CombatMovement);
        }
    }

    public override void Exit()
    {
        
    }
}
