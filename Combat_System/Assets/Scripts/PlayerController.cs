using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    private bool isGrounded;

    private float ySpeed;

    void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;

        //人物朝向当前摄像机的方向移动
        var moveDir = cameraController.PlanarRotation() * moveInput;

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
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);

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
        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
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
    /// 为了在编辑模式下可以看清物理检测的范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
