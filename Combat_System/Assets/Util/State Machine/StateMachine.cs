using UnityEngine;

public class StateMachine<T>
{
    public State<T> CurrentState { get; private set; }

    T _owner;

    //对于不同ai（敌人/NPC）的fsm有不同的控制器
    public StateMachine(T owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// 状态切换
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(State<T> newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter(_owner);
    }

    /// <summary>
    /// 在当前状态内要执行的函数
    /// </summary>
    public void Execute()
    {
        CurrentState?.Execute();
    }
}
