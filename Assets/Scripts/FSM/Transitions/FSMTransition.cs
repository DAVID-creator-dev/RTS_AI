using System;
using UnityEngine;

[Serializable]
public class FSMTransition
{
    public FSMCondition condition;
    public FSMState to;

    public virtual void EnterTransition()
    {
        //stuff
    }
    public virtual void ExitTransition()
    {
        //stuff
    }
    public virtual bool CanTransition(FSM controller)
    {
        return condition.CheckCondition(controller);
    }
}
