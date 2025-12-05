using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;
    public static PlayerController Instance => instance;

    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float rotationSpeed = 5;

    [Header("地面检测")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;

    private CameraController cameraController;
    private Quaternion targetRotation;
    private Animator animator;
    private CharacterController characterController;
    private MeeleFighter meeleFighter;
    private CombatController combatController;

    private bool isGrounded;
    private bool hasControl;

    private float ySpeed;

    public float RotationSpeed => rotationSpeed;

    public Vector3 InputDir { get; private set; }

    void Awake()
    {
        //游戏开始人物是可以控制的
        hasControl = true;

        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();
        combatController = GetComponent<CombatController>();

        if(instance == null)
        {
            instance = this;
        }
    }

    void Update()
    {
        //当攻击正在进行时，停止人物的逻辑
        if (meeleFighter.InAction)
        {
            //玩家进行反击的时候 角度的旋转得记录下来
            targetRotation = transform.rotation;
            animator.SetFloat("forwardSpeed", 0);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;

        //人物朝向当前摄像机的方向移动
        var moveDir = cameraController.PlanarRotation() * moveInput;
        //在锁敌模式下可以根据输入方向来进行攻击
        InputDir = moveDir;
        
        //当在播放其它动画时 人物不受其它按键控制
        if(!hasControl) return;

        GroundCheck();

        if (isGrounded)
        {
            //确保玩家可以黏在地上（下坡时）
            ySpeed = -3f;
        }
        else
        {
            //模拟在重力下自由落体
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        //y方向的速度受重力影响
        var velocity = moveDir * moveSpeed;

        //根据是否锁敌 切换移动模式
        if (combatController.CombatMode)
        {
            //锁敌状态只能走动 不能跑动
            velocity /= 4f;

            //始终面向锁定的敌人
            var targetVec = combatController.TargetEnemy.transform.position - transform.position;
            targetVec.y = 0;

            if(moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(targetVec);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            //移动分为前后与左右
            // |v| * cosθ 敌人面朝向的速度
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);      
            animator.SetFloat("forwardSpeed", forwardSpeed / moveSpeed, 0.2f, Time.deltaTime);

            float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
            //侧移速度 这里只要-1 ~ 1的数据即可
            float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
            animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            //判断当前是否可以移动
            if(moveAmount > 0)
            {
                //characterController.Move(moveDir * moveSpeed * Time.deltaTime);
                //transform.position += moveDir * moveSpeed * Time.deltaTime;
                targetRotation = Quaternion.LookRotation(moveDir);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            //第三个参数：dampTime：阻尼时间，控制变化的平滑程度（越大越慢）。
            //第四个参数：deltaTime：通常传入 Time.deltaTime，确保平滑计算与帧率无关。
            animator.SetFloat("forwardSpeed", moveAmount, 0.2f, Time.deltaTime);
        }

        //重力的速度不受玩家影响
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 检测当前玩家是否在地面
    /// </summary>
    private void GroundCheck()
    {
        // transform.TransformPoint(groundCheckOffset) 把相对当前玩家的局部位置转为世界位置
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    /// <summary>
    /// 让外部设置人物当前是否可以控制
    /// </summary>
    /// <param name="hasControl"></param>
    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        //碰撞检测失效
        characterController.enabled = hasControl;

        //如果当前人物无法控制 就停止一切运动和旋转
        if (!hasControl)
        {
            animator.SetFloat("forwardSpeed", 0);
            targetRotation = transform.rotation;
        }
    }

    /// <summary>
    /// 如果InputDir为Vector3.zero，强制转为面朝向
    /// </summary>
    /// <returns></returns>
    public Vector3 GetIntentDirection()
    {
        return InputDir != Vector3.zero ? InputDir : transform.forward;
    } 

    /// <summary>
    /// 为了在编辑模式下可以看清物理检测的范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
