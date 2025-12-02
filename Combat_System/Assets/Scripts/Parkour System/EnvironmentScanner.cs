using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardRayOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float forwardRayLength = 0.8f;
    [SerializeField] private float heightRayLength = 5f;
    [SerializeField] private LayerMask obstacleLayer;

    /// <summary>
    /// 检测人物面前的障碍物
    /// </summary>
    /// <returns></returns>
    public ObstacleHitDate ObstacleCheck()
    {
        var hitData = new ObstacleHitDate();

        var forwardOrigin = transform.position + forwardRayOffset;
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit, 
                                        forwardRayLength, obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength, 
                      (hitData.forwardHitFound) ? Color.red : Color.white);

        //前方有物体才会去显示高度射线
        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;

            //射线检测到的就是前方障碍，因此可以通过hitData.heightHit得到障碍高度
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHit,
                                                heightRayLength, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, 
                          (hitData.heightHitFound) ? Color.red : Color.white);
        }

        return hitData;
    }
}

/// <summary>
/// 射线检测到的障碍数据
/// </summary>
public struct ObstacleHitDate
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
}
