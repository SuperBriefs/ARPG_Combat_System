using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class EnemyAttackState : State<EnemyController>
{
    [SerializeField] private float attackDistance = 1f;

    private bool isAttacking;

    private EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;

        enemy.NavAgent.stoppingDistance = attackDistance;
    }

    public override void Execute()
    {
        //正在攻击就直接退出
        if(isAttacking) return;

        enemy.NavAgent.SetDestination(enemy.Target.transform.position);

        // +0.03f 是因为attackDistance的距离不是百分百准确的，需要来一点容错
        if(Vector3.Distance(enemy.Target.transform.position,  enemy.transform.position) <= attackDistance + 0.03f)
        {
            StartCoroutine(Attack(Random.Range(1, enemy.Fighter.Attacks.Count + 1)));
        }
    }

    float timer = 0.5f;
    IEnumerator Attack(int comboCount = 1)
    {
        isAttacking = true;
        //攻击使用根运动
        enemy.Animator.applyRootMotion = true;

        enemy.Fighter.TryToAttack(enemy.Target);

        //连击
        for(int i = 1; i < comboCount; i++)
        {
            //攻击进入冷却状态后 再次攻击 则进行连击
            //如果0.5s还没有进入冷却状态 说明攻击被断了 直接返回即可
            yield return new WaitUntil(() => 
            {
                timer -= Time.deltaTime; 
                return enemy.Fighter.AttackState == AttackState.Cooldown || timer <= 0;
            });

            //被打断退出连招
            if(timer <= 0)
            {
                break;
            }

            enemy.Fighter.TryToAttack(enemy.Target);
            timer = 0.5f;
        }

        yield return new WaitUntil(() => enemy.Fighter.AttackState == AttackState.Idle);

        enemy.Animator.applyRootMotion = false;
        isAttacking = false;

        //打完就进入后退状态 如果角色死亡不会切换状态
        if(enemy.IsInState(EnemyStates.Attack))
        {
            enemy.ChangeState(EnemyStates.RetreatAfterAttack);
        }
    }

    public override void Exit()
    {
        enemy.NavAgent.ResetPath();
    }
}
