using System.Collections.Generic;
using System;
using UnityEngine;

namespace MPCore
{
    /// <summary> Allows you to assign methods as the start, update, and end of states. </summary>
    public class StateMachine
    {
        private readonly Dictionary<string, State> stateList = new Dictionary<string, State>();
        private State currentState;

        /// <summary> Create a new state and set up its associated methods. </summary>
        public void Add(params State[] states)
        {
            foreach (State state in states)
                stateList.Add(state.name, state);
        }

        /// <summary> Delete a state by name. </summary>
        public void RemoveState(string name)
        {
            stateList.Remove(name);
        }

        /// <summary> Switch to a new state. Does not prevent same-state switching. </summary>
        public void SwitchTo(string name, bool resetIfSame = false)
        {
            if (stateList.TryGetValue(name, out State newState) && !resetIfSame || !IsCurrentState(name))
                SetState(newState);
        }

        public void SetState(State newState)
        {
            currentState.end?.Invoke();
            currentState = newState;
            currentState.startTime = Time.time;
            currentState.start?.Invoke();
        }

        /// <summary> Initialize the state system. </summary>
        public void Initialize(string name)
        {
            if (stateList.TryGetValue(name, out State s))
                (currentState = s).start?.Invoke();
        }

        /// <summary> Update the current state. </summary>
        public void Update()
        {
            currentState.update?.Invoke();
        }

        /// <summary> FixedUpdate the current state. </summary>
        public void FixedUpdate()
        {
            currentState.fixedUpdate?.Invoke();
        }

        /// <summary> Time (s) of current state duration. </summary>
        public float StateTime => Time.time - currentState.startTime;

        /// <summary> Check what the current state is. </summary>
        public bool IsCurrentState(string name)
        {
            return currentState.name?.Equals(name) ?? false;
        }
    }

    public class State
    {
        public float startTime;
        public readonly string name;
        public readonly Action start, update, fixedUpdate, end;

        public State(string name, Action start = null, Action update = null, Action fixedUpdate = null, Action end = null)
        {
            startTime = Time.time;
            this.name = name;
            this.start = start;
            this.update = update;
            this.fixedUpdate = fixedUpdate;
            this.end = end;
        }
    }
}
