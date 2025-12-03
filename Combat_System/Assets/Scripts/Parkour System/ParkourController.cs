using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] private List<ParkourAction> parkourActions;

    private EnvironmentScanner environmentScanner;
    private Animator animator;
    private PlayerController playerController;
    private MeeleFighter meeleFighter;

    private bool inAction;
    public bool InAction => inAction;

    void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        meeleFighter = GetComponent<MeeleFighter>();
    }

    void Update()
    {
        //按下空格键，且当前不在动作中
        if(Input.GetKeyDown(KeyCode.Space) && !inAction && !meeleFighter.InAction)
        {
            var hitDate = environmentScanner.ObstacleCheck();
            if (hitDate.forwardHitFound)
            {
                //遍历所有动画 找到满足条件的就去播放
                foreach(var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitDate, transform))
                    {
                        StartCoroutine(DoparkourAction(action));
                        break;
                    }
                }
            }
        }
    }

    IEnumerator DoparkourAction(ParkourAction action)
    {
        inAction = true;
        playerController.SetControl(false);

        //过渡时长受动画影响 一般0.2f就可以 但是对于跨栏的动画过度时长得短一些，不然会影响到动作匹配
        animator.CrossFade(action.AnimName, action.CrossFadeTime);
        //由于CrossFade会在下一帧后才会开始切换动画 所以要等一帧再执行后面的内容
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        //以防名字对应错误
        if (!animState.IsName(action.AnimName))
        {
            Debug.LogError("The parkour animation is wrong!");
        }

        //等待过渡完成后，再执行后面的代码
        //这样可以避免在过渡状态进行动作匹配而导致匹配失效
        while (animator.IsInTransition(0))
            yield return null;

        //播放动画后 等待动画时长后 动画结束
        //yield return new WaitForSeconds(animState.length);
        float timer = 0;
        while(timer <= animState.length)
        {
            timer += Time.deltaTime;

            //玩家向障碍旋转
            if (action.RotateToObstacle)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, action.TargetRotation, Time.deltaTime * playerController.RotationSpeed);
            }

            //动作目标匹配
            if (action.EnableTargetMatching)
            {
                MatchTaregt(action);
            }

            if(animator.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }

            yield return null;
        }

        //等待一段时间再还给玩家控制权（主要给攀爬动画后的站立动画使用）
        yield return new WaitForSeconds(action.PostActionDelay);
        
        playerController.SetControl(true);
        inAction = false;
    }

    /// <summary>
    /// 用于进行动作目标匹配的函数
    /// </summary>
    /// <param name="action"></param>
    private void MatchTaregt(ParkourAction action)
    {
        //如果Animator当前已经在做MatchTarget，就直接返回，避免重复调用
        if(animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart,
                             new MatchTargetWeightMask(action.MatchPosWeight, 0), //只有y轴方向进行匹配，角度不旋转
                                                                                 //对于攀爬动作还需要z轴一起匹配抓取的位置
                             action.MatchStartTime, action.MatchTargetTime);
    }
}
