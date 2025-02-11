using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum States
{
    Pursuit,
    Pathfinding,
    Avoid
}

public class State<T>
{
    public string name { get { return _stateName; } }

    public event Action<T> OnEnter = delegate { };
    public event Action OnUpdate = delegate { };
    public event Action OnLateUpdate = delegate { };
    public event Action OnFixedUpdate = delegate { };
    public event Action<T> OnExit = delegate { };


    private string _stateName;
    private Dictionary<T, Transition<T>> transitions;

    public State(string name)
    {
        _stateName = name;
    }

    public State<T> Configure(Dictionary<T, Transition<T>> t)
    {
        transitions = t;
        return this;
    }

    public Transition<T> GetTransition(T input)
    {
        return transitions[input];
    }


    public bool CheckInput(T input, out State<T> next)
    {
        if (transitions.ContainsKey(input))
        {
            var trans = transitions[input];
            trans.OnTransitionExecute(input);
            next = trans.TargetState;
            return true;
        }

        next = this;
        return false;

    }

    public void Enter(T input)
    {
        OnEnter(input);
    }

    public void Update()
    {
        OnUpdate();
    }

    public void LateUpdate()
    {
        OnLateUpdate();
    }

    public void FixedUpdate()
    {
        OnFixedUpdate();
    }

    public void Exit(T input)
    {
        OnExit(input);
    }
}
