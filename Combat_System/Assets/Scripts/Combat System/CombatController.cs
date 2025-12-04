using UnityEngine;

public class CombatController : MonoBehaviour
{
    public EnemyController targetEnemy;

    private MeeleFighter meeleFighter;
    private Animator animator;
    private CameraController cam;

    void Awake()
    {
        meeleFighter = GetComponent<MeeleFighter>();
        animator = GetComponent<Animator>();
        cam = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var enemy = EnemyManager.Instance.GetAttackingEnemy();
            if(enemy != null && enemy.Fighter.IsCounterable && !meeleFighter.InAction && meeleFighter.InVisionToCounter(enemy))
            {
                StartCoroutine(meeleFighter.PerformCounterAttack(enemy));
            }

            meeleFighter.TryToAttack();
        }
    }

    /// <summary>
    /// 当Animator使用Root Motion时，每一帧都会调用这个方法，这样就可以手动控制动画驱动的位移和旋转
    /// </summary>
    void OnAnimatorMove()
    {
        //玩家反击的时候 不使用根驱动位移
        if(!meeleFighter.InCounter)
        {
            transform.position += animator.deltaPosition;
        }
        transform.rotation *= animator.deltaRotation;
    }

    /// <summary>
    /// 得到玩家视线方向 就是相机与玩家之间向量的水平分量
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTargetingDir()
    {
        var vecFromCam = transform.position - cam.transform.position;
        vecFromCam.y = 0;
        return vecFromCam.normalized;
    }
}
