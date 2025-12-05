using System.Collections;
using UnityEngine;

public class GettingHitState : State<EnemyController>
{
    [SerializeField] private float stunnTime = 0.5f;

    private EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        //停止之前的所有协程，以免出现多次击晕
        StopAllCoroutines();

        enemy = owner;

        enemy.Fighter.OnHitComplete += OnHitCompleteEvent;
    }

    /// <summary>
    /// 击晕的事件
    /// </summary>
    private void OnHitCompleteEvent()
    {
        StartCoroutine(GoToCombatMovement());
    }

    /// <summary>
    /// 模拟击晕状态
    /// </summary>
    /// <returns></returns>
    IEnumerator GoToCombatMovement()
    {
        yield return new WaitForSeconds(stunnTime);
        enemy.ChangeState(EnemyStates.CombatMovement);
    }

    public override void Exit()
    {
        enemy.Fighter.OnHitComplete -= OnHitCompleteEvent;
    }
}
