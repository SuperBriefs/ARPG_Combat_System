using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New parkour action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] private string animName;
    [SerializeField] private string obstacleTag;
    [SerializeField] private float crossFadeTime;

    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;

    [SerializeField] private bool rotateToObstacle;
    [SerializeField] private float postActionDelay;

    [Header("动作目标匹配")]
    [SerializeField] private bool enableTargetMatching = true;
    [SerializeField] private AvatarTarget matchBodyPart;
    [SerializeField] private float matchStartTime;
    [SerializeField] private float matchTargetTime;
    [SerializeField] private Vector3 matchPosWeight = new Vector3(0, 1, 0);

    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPos { get; set; }

    /// <summary>
    /// 提供给外部判断当前障碍是否可以跨越
    /// </summary>
    /// <param name="hitDate">检测到的障碍</param>
    /// <param name="player">玩家</param>
    /// <returns></returns>
    public bool CheckIfPossible(ObstacleHitDate hitDate, Transform player)
    {
        //标签不为空 且 当前检测到的物体标签不是obstacleTag 就不去执行后续的动画播放
        if (!string.IsNullOrEmpty(obstacleTag) && hitDate.forwardHit.transform.tag != obstacleTag)
        {
            return false;
        }

        float height = hitDate.heightHit.point.y - player.position.y;
        if(height < minHeight || height > maxHeight)
        {
            return false;
        }

        if (rotateToObstacle)
        {
            TargetRotation = Quaternion.LookRotation(-hitDate.forwardHit.normal);
        }

        if (enableTargetMatching)
        {
            MatchPos = hitDate.heightHit.point;
        }
        
        return true;
    }

    public string AnimName => animName;
    public float CrossFadeTime => crossFadeTime;

    public bool RotateToObstacle => rotateToObstacle;
    public float PostActionDelay => postActionDelay;

    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPosWeight => matchPosWeight;
}
