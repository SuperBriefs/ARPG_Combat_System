using UnityEngine;

public class DeadState : State<EnemyController>
{
    public override void Enter(EnemyController owner)
    {
        //关闭敌人视觉触发器VisionSensor，避免敌人再一次加入可攻击敌人的列表
        owner.VisionSensor.gameObject.SetActive(false);
        //将敌人移出可攻击敌人的列表中
        EnemyManager.Instance.RemoveEnemyInRange(owner);

        //敌人身上的寻路与人物控制器全部失活，避免敌人死后还能对攻击有反应
        owner.NavAgent.enabled = false;
        owner.CharacterController.enabled = false;
    }
}
