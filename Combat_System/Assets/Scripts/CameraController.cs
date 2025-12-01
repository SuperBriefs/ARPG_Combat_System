using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform followTarget;

    [SerializeField] private float rotationSpeed = 2;
    [SerializeField] private float distance = 5;
    [SerializeField] private float minVerticalAngle = -45;
    [SerializeField] private float maxVerticalAngle = 45;

    [SerializeField] private Vector2 framingOffset;

    //相机是否颠倒
    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY;

    private float rotationX;
    private float rotationY;

    private float invertXVal;
    private float invertYVal;

    void Start()
    {
        //游戏一开始就隐藏并锁定鼠标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //相机移动方向是否要颠倒
        invertXVal = invertX ? -1 : 1;
        invertYVal = invertY ? -1 : 1;

        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;

        //摄像机旋转
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        //摄像机看的位置应该在人物胸部高度，不应该在脚底，需要增加一个偏移量
        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y, 0);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.LookAt(focusPosition);
    }

    /// <summary>
    /// 当人物移动时，只允许人物相对相机水平面角度移动
    /// </summary>
    /// <returns></returns>
    public Quaternion PlanarRotation()
    {
        return Quaternion.Euler(0, rotationY, 0);
    }
}
