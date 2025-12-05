using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackState { Idle, Windup, Impact, Cooldown};

public class MeeleFighter : MonoBehaviour
{
    [SerializeField] private GameObject sword;

    [SerializeField] private List<AttackData> attacks;
    [SerializeField] private List<AttackData> longRangeAttacks;
    [SerializeField] private float longRangeAttackThreshold = 1.5f;
    [SerializeField] private float rotationSpeed = 500;

    public event Action<MeeleFighter> OnGoHit;
    public event Action OnHitComplete;

    private BoxCollider swordCollider;
    private SphereCollider leftHandCollider,rightHandCollider,leftFootCollider,rightFootCollider;

    private Animator animator;
    private ParkourController parkourController;
    
    private bool inAction = false;
    private bool doCombo;
    private int comboCount = 0;
    private float Fov = 180f;
    
    public bool InAction => inAction;
    public AttackState AttackState { get; private set; }
    public bool InCounter { get; set; } = false;
    public bool IsTakingHit { get; private set; } = false;

    void Awake()
    {
        AttackState = AttackState.Idle;

        animator = GetComponent<Animator>();
        parkourController = GetComponent<ParkourController>();
    }

    void Start()
    {
        //一开始禁用武器触发器
        if(sword != null)
        {
            swordCollider = sword.GetComponent<BoxCollider>();
            leftHandCollider = animator.GetBoneTransform(HumanBodyBones.LeftHand).GetComponent<SphereCollider>();
            rightHandCollider = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponent<SphereCollider>();
            rightFootCollider = animator.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<SphereCollider>();
            leftFootCollider = animator.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<SphereCollider>();

            //一开始所有攻击触发器都失活
            DisableAllHitboxes();
        }
    }

    /// <summary>
    /// 尝试进行攻击
    /// </summary>
    public void TryToAttack(MeeleFighter target = null)
    {
        //敌人没有跑酷系统
        if (!inAction && (parkourController == null || !parkourController.InAction))
        {
            StartCoroutine(Attack(target));
        }
        else if ((AttackState == AttackState.Impact || AttackState == AttackState.Cooldown) && (parkourController == null || !parkourController.InAction))
        {
            doCombo = true;
        }
    }

    MeeleFighter currTarget;
    IEnumerator Attack(MeeleFighter target = null)
    {
        inAction = true;
        //这里不能用跑酷系统时取消人物控制的逻辑 因为这会取消人物CharacterController，而这里只想取消人物移动
        //playerController.SetControl(false);

        //设置单一攻击目标 每次攻击只能打到一个敌人
        currTarget = target;
        AttackState = AttackState.Windup;

        var attack = attacks[comboCount];
        Vector3 startPos = transform.position;
        Vector3 targetPos = Vector3.zero;
        //玩家有攻击目标时 确定目标方向 进行远距离攻击
        var attackDir = transform.forward;
        if(target != null)
        {
            var vecToTarget = target.transform.position - transform.position;
            vecToTarget.y = 0;

            attackDir = vecToTarget.normalized;
            float distance = vecToTarget.magnitude - attack.DistanceFromTarget;

            //大于longRangeAttackThreshold距离 进行远距离攻击
            if(distance > longRangeAttackThreshold && longRangeAttacks.Count > 0)
            {
                attack = longRangeAttacks[0];
            }

            if(attack.MoveToTarget)
            {
                if(distance <= attack.MaxMoveDistance)
                {
                    targetPos = target.transform.position - attackDir * attack.DistanceFromTarget;
                }
                else
                {
                    targetPos = startPos + attackDir * attack.DistanceFromTarget;
                }
            }
        }

        //攻击进行检测的开始与结束时间（归一）
        // float impactStartTime = 0.33f;
        // float impactEndTime = 0.55f;

        animator.CrossFade(attack.AnimName, 0.2f);
        yield return null;

        //战斗系统的动画在1层
        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            //攻击时受击 直接退出播放动画时长的循环
            if(IsTakingHit) break;

            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            //玩家进行远距离攻击时 向敌人方向进攻
            if(target != null && attack.MoveToTarget)
            {
                float percTime = (normalizedTime - attack.MoveStartTime) / (attack.MoveEndTime - attack.MoveStartTime);
                transform.position = Vector3.Lerp(startPos, targetPos, percTime);
            }

            //转向攻击方向
            if(attackDir != null)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(attackDir), rotationSpeed * Time.deltaTime);
            }

            if (AttackState == AttackState.Windup)
            {
                //被反击了就退出
                if(InCounter) break;

                if(normalizedTime >= attack.ImpactStartTime)
                {
                    AttackState = AttackState.Impact;
                    //激活武器触发器
                    EnableHitbox(attack);
                }
            }
            else if(AttackState == AttackState.Impact)
            {
                if(normalizedTime >= attack.ImpactEndTime)
                {
                    AttackState = AttackState.Cooldown;
                    //失活武器触发器
                    DisableAllHitboxes();
                }
            }
            else if (AttackState == AttackState.Cooldown)
            {
                //TODO: 处理连击的逻辑
                if (doCombo)
                {
                    doCombo = false;
                    comboCount = (comboCount + 1) % attacks.Count;

                    //可以进行连击的话就再开一个攻击协程 并 结束当前的协程
                    StartCoroutine(Attack(target));
                    yield break;
                }
            }

            yield return null;
        }

        AttackState = AttackState.Idle;

        //攻击重头开始
        comboCount = 0;

        inAction = false;
        currTarget = null;
    }

    void OnTriggerEnter(Collider other)
    {
        //敌人检测到玩家的攻击(玩家的武器 敌人)
        if(other.gameObject.layer == LayerMask.NameToLayer("PlayerHitBox") && 
           this.gameObject.layer == LayerMask.NameToLayer("Enemy") &&
           !IsTakingHit /*在攻击的时候可以受击*/)
        {
            //敌人只能攻击单一玩家
            AttackOtherSide(other);
        }

        //玩家检测敌人的攻击(敌人的武器 玩家)
        if(other.gameObject.layer == LayerMask.NameToLayer("EnemyHitBox") && 
           this.gameObject.layer == LayerMask.NameToLayer("Player") &&
           !IsTakingHit &&
           !InCounter /*玩家反击时，不会受击*/)
        {
            //玩家只能攻击单一敌人
            AttackOtherSide(other);
        }
    }

    /// <summary>
    /// 受击状态 且 每次攻击受击的只有一个角色
    /// </summary>
    /// <param name="other"></param>
    private void AttackOtherSide(Collider other)
    {
        var attacker = other.GetComponentInParent<MeeleFighter>();
        if(attacker.currTarget != this)
        {
            return;
        }

        StartCoroutine(PlayHitReaction(attacker));
    }

    IEnumerator PlayHitReaction(MeeleFighter attacker)
    {
        inAction = true;
        //这里不能用跑酷系统时取消人物控制的逻辑 因为这会取消人物CharacterController，而这里只想取消人物移动
        //playerController.SetControl(false);

        IsTakingHit = true;

        //受击转向攻击者
        var dispVec = attacker.transform.position - transform.position;
        dispVec.y = 0;
        transform.rotation = Quaternion.LookRotation(dispVec);

        //进入受击后的击晕状态 敌人执行
        //进入受击后切换目标 玩家执行
        OnGoHit?.Invoke(attacker);

        animator.CrossFade("SwordImpact", 0.2f);
        yield return null;

        //战斗系统的动画在1层
        var animState = animator.GetNextAnimatorStateInfo(1);

        //等待80%的时间即可再次播放
        yield return new WaitForSeconds(animState.length * 0.8f);

        //玩家攻击结束后 敌人退出击晕状态
        OnHitComplete?.Invoke();

        inAction = false;
        IsTakingHit = false;
    }

    /// <summary>
    /// 玩家进行反击 敌人触发被反击死亡动画
    /// </summary>
    /// <param name="opponent"></param>
    /// <returns></returns>
    public IEnumerator PerformCounterAttack(EnemyController opponent)
    {
        inAction = true;

        InCounter = true;
        opponent.Fighter.InCounter = true;
        opponent.ChangeState(EnemyStates.Dead);

        //玩家和敌人要面对面
        var dispVec = opponent.transform.position - transform.position;
        dispVec.y = 0;
        transform.rotation = Quaternion.LookRotation(dispVec);
        opponent.transform.rotation = Quaternion.LookRotation(-dispVec);

        //玩家移动到敌人身前1m
        var targetPos = opponent.transform.position - dispVec.normalized * 1f;

        animator.CrossFade("CounterAttack", 0.2f);
        opponent.Animator.CrossFade("CounterAttackVictim", 0.2f);
        yield return null;

        //战斗系统的动画在1层
        var animState = animator.GetNextAnimatorStateInfo(1);
        
        float timer = 0f;
        while (timer <= animState.length)
        {
            //玩家移动到targetPos的位置，避免玩家与敌人位置重叠
            transform.position = Vector3.Lerp(transform.position, targetPos, 5 * Time.deltaTime);

            yield return null;

            timer += Time.deltaTime;
        }

        InCounter = false;
        opponent.Fighter.InCounter = false;

        inAction = false;
    }

    /// <summary>
    /// 激活当前攻击使用的触发器
    /// </summary>
    private void EnableHitbox(AttackData attack)
    {
        switch (attack.HitboxToUse)
        {
            case AttackHitbox.LeftHand:
                leftHandCollider.enabled = true;
                break;
            case AttackHitbox.RightHand:
                rightHandCollider.enabled = true;
                break;
            case AttackHitbox.LeftFoot:
                leftFootCollider.enabled = true;
                break;
            case AttackHitbox.RightFoot:
                rightFootCollider.enabled = true;
                break;
            case AttackHitbox.Sword:
                swordCollider.enabled = true;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 让所有攻击触发器都失活
    /// </summary>
    private void DisableAllHitboxes()
    {
        if(swordCollider != null)
            swordCollider.enabled = false;

        if(leftHandCollider != null)
            leftHandCollider.enabled = false; 
        if(rightHandCollider != null)
            rightHandCollider.enabled = false;
        if(rightFootCollider != null)
            rightFootCollider.enabled = false;
        if(leftFootCollider!=null)
            leftFootCollider.enabled = false;
    }

    /// <summary>
    /// 敌人是否在玩家视野范围内 在才可以反击
    /// </summary>
    /// <returns></returns>
    public bool InVisionToCounter(EnemyController opponent)
    {
        var dispVec = opponent.transform.position - transform.position;
        var angle = Vector3.Angle(transform.forward, dispVec);

        if(angle <= Fov / 2)
        {
            return true;
        }
        return false;
    }

    public List<AttackData> Attacks => attacks;

    //武器处于抬起状态时并且攻击为第一次攻击时 玩家可以进行反击
    public bool IsCounterable => AttackState == AttackState.Windup && comboCount == 0;
}
