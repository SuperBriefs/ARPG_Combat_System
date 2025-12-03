using UnityEngine;

[CreateAssetMenu(menuName = "Combat System/create a new attack")]
public class AttackData : ScriptableObject
{
    //需要在Inspector设置，但是序列化的时候会忽略
    [field: SerializeField] public string AnimName { get; private set; }
    [field: SerializeField] public AttackHitbox HitboxToUse { get; private set; }
    [field: SerializeField] public float ImpactStartTime { get; private set; }
    [field: SerializeField] public float ImpactEndTime { get; private set; }
}

public enum AttackHitbox { LeftHand, RightHand, LeftFoot, RightFoot, Sword };
