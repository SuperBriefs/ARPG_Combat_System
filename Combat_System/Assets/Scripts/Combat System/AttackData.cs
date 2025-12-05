using UnityEngine;

[CreateAssetMenu(menuName = "Combat System/create a new attack")]
public class AttackData : ScriptableObject
{
    //需要在Inspector设置，但是序列化的时候会忽略
    [field: SerializeField] public string AnimName { get; private set; }
    [field: SerializeField] public AttackHitbox HitboxToUse { get; private set; }
    [field: SerializeField] public float ImpactStartTime { get; private set; }
    [field: SerializeField] public float ImpactEndTime { get; private set; }

    [field: Header("向敌人方向攻击")]
    [field: SerializeField] public bool MoveToTarget { get; private set; }
    [field: SerializeField] public float DistanceFromTarget { get; private set; } = 1f; /*避免玩家和敌人重叠的距离*/
    [field: SerializeField] public float MaxMoveDistance { get; private set; } = 3f; /*玩家可以进行攻击的最远距离*/
    [field: SerializeField] public float MoveStartTime { get; private set; } = 0; /*MoveStartTime和MoveEndTime是标准化时间*/
    [field: SerializeField] public float MoveEndTime { get; private set; } = 1; /*攻击开始位移和结束位移的时间*/
}

public enum AttackHitbox { LeftHand, RightHand, LeftFoot, RightFoot, Sword };
