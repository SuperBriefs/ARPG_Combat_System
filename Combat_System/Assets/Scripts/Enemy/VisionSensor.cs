using UnityEngine;

public class VisionSensor : MonoBehaviour
{
    [SerializeField] private EnemyController enemy;

    /// <summary>
    /// 由于设置了检测的层级 所以现在触发器只会被Player触发
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        //添加追击的目标
        var fighter = other.GetComponent<MeeleFighter>();
        if(fighter != null)
        {
            enemy.TargetsInRange.Add(fighter);
        }
    }

    void OnTriggerExit(Collider other)
    {
        //移除追击的目标
        var fighter = other.GetComponent<MeeleFighter>();
        if(fighter != null)
        {
            enemy.TargetsInRange.Remove(fighter);
        }
    }
}
