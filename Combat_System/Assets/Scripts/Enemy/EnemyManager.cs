using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Vector2 timeRangeBetweenAttacks = new Vector2(1, 4);
    [SerializeField] private CombatController player;

    private static EnemyManager instance;
    public static EnemyManager Instance => instance;

    private List<EnemyController> enemiesInRange;
    private float notAttackingTimer = 2f;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        enemiesInRange = new List<EnemyController>();
    }

    /// <summary>
    /// 加入可攻击敌人
    /// </summary>
    /// <param name="enemy"></param>
    public void AddEnemyInRange(EnemyController enemy)
    {
        //一个敌人只能加入一次
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }
    }

    /// <summary>
    /// 删除可攻击敌人
    /// </summary>
    /// <param name="enemy"></param>
    public void RemoveEnemyInRange(EnemyController enemy)
    {
        if (enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
        }
    }

    float timer = 0;
    void Update()
    {
        if(enemiesInRange.Count == 0) return;

        //查找当前是否有在攻击的敌人
        bool hasAttackingEnemy = false;
        foreach (var e in enemiesInRange)
        {
            if (e.IsInState(EnemyStates.Attack))
            {
                hasAttackingEnemy = true;
                break;
            }
        }

        if (!hasAttackingEnemy)
        {
            if(notAttackingTimer > 0)
            {
                notAttackingTimer -= Time.deltaTime;
            }

            if(notAttackingTimer <= 0)
            {
                //攻击玩家
                var attackingEnemy = SelectEnemyForEnemy();

                if(attackingEnemy != null)
                {
                    attackingEnemy.ChangeState(EnemyStates.Attack);
                    notAttackingTimer = Random.Range(timeRangeBetweenAttacks.x, timeRangeBetweenAttacks.y);
                }
            }
        }

        //避免每帧都去找一次
        if(timer >= 0.1f)
        {
            timer = 0;
            //离玩家最近的可攻击的敌人
            var closestEnemy = GetClosesEnemyToPlayerDir();
            if(closestEnemy != null && closestEnemy != player.targetEnemy)
            {
                //先取消上一次最近的敌人的高光
                var prevEnemy = player.targetEnemy;
                prevEnemy?.MeshHighlighter.HighlightMesh(false);

                player.targetEnemy = closestEnemy;
                player.targetEnemy?.MeshHighlighter.HighlightMesh(true);
            }
        }

        timer += Time.deltaTime;
    }

    /// <summary>
    /// 选择敌人攻击玩家
    /// </summary>
    /// <returns></returns>
    private EnemyController SelectEnemyForEnemy()
    {
        //返回处于战斗运动状态最长的敌人 且 当前敌人得有目标
        return enemiesInRange.OrderByDescending(e => e.CombatMovementTimer).FirstOrDefault(e => e.Target != null);
    }

    /// <summary>
    /// 获得当前攻击的敌人
    /// </summary>
    /// <returns></returns>
    public EnemyController GetAttackingEnemy()
    {
        return enemiesInRange.FirstOrDefault(e => e.IsInState(EnemyStates.Attack));
    }

    /// <summary>
    /// 得到离玩家最近的敌人
    /// </summary>
    /// <returns></returns>
    public EnemyController GetClosesEnemyToPlayerDir()
    {
        var targetingDir = player.GetTargetingDir();

        float minDistance = Mathf.Infinity;
        EnemyController closestEnemy = null;

        foreach(var enemy in enemiesInRange)
        {
            var vecToEnemy = enemy.transform.position - player.transform.position;
            vecToEnemy.y = 0;

            //|v| * sinθ
            float angle = Vector3.Angle(targetingDir, vecToEnemy);
            float distance = vecToEnemy.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

            if(minDistance > distance) 
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }
}
