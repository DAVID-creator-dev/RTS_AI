using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FSM : MonoBehaviour
{

    private FSMState currentState;
    private FSMState previousState;
    private float currentUpdate = 0.0f;

    public List<FSMState> states = new List<FSMState>();
    public float updateFrequency = 0.1f;

    private Unit owner;
    public Unit GetOwner() { return owner; }


    private void Start()
    {
        owner = transform.parent.GetComponent<Unit>();
        states = GetComponentsInChildren<FSMState>().ToList<FSMState>();
        if (states.Count == 0)
            return;

        InitStates();
    }

    private void Update()
    {
        if (currentUpdate > Time.time)
            return;
        currentUpdate = Time.time + updateFrequency;
        UpdateStates();
    }

    private void InitStates()
    {
        foreach (FSMState state in states)
        {
            state.Create(this);
        }
        currentState = states[0];
        states[0].EnterState();
    }


    private void UpdateStates()
    {
        if (currentState == null)
            return;

        FSMState newState = currentState.UpdateState();
        if (newState != null && newState != currentState)
        {
            currentState.ExitState();
            newState.EnterState();

            previousState = currentState;
            currentState = newState;
        }
    }
}
