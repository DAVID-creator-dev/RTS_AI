using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FSMState : MonoBehaviour
{
    public List<FSMTransition> transitions = new List<FSMTransition>();
    private FSM controllerParent;
    public FSM GetController() {  return controllerParent; }
    

    public virtual void Create(FSM controller)
    {
        controllerParent = controller;
    }

    public virtual void EnterState()
    {
        //stuff
    }
    public virtual void ExitState()
    {
        //stuff
    }
    public virtual FSMState UpdateState()
    {
        FSMTransition validTransition = CheckTransitions();
        if (validTransition != null)
            return validTransition.to;
        else
            return this;
    }

    private FSMTransition CheckTransitions()
    {
        foreach (FSMTransition transition in transitions)
        {
            if (transition.CanTransition(controllerParent))
                return transition;
        }
        return null;
    }

}
