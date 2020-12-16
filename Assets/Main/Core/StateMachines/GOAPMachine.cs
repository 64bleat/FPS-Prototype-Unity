using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MPCore
{
    public enum GOAPStatus { Running, Continue, Fail }

    public class GOAP
    {
        public readonly List<IGOAPAction> actions = new List<IGOAPAction>();
        private Stack<IGOAPAction> actionStack;

        private IGOAPAction CurrentAction => actionStack?.Count > 0 ? actionStack.Peek() : null;

        public void AddActions(params IGOAPAction[] goapActions)
        {
            foreach(IGOAPAction goapAction in goapActions)
                actions.Add(goapAction);
        }

        public void InjectPredecessor(params IGOAPAction[] actions)
        {
            foreach(IGOAPAction action in actions)
                actionStack.Push(action);
        }

        public void GOAPUpdate()
        {
            switch (CurrentAction != null ? CurrentAction.Update() : GOAPStatus.Fail)
            {
                case GOAPStatus.Running:
                    break;

                case GOAPStatus.Continue:
                    IGOAPAction action = actionStack?.Pop();
                    action?.OnEnd();
                    CurrentAction?.OnStart();
                    break;

                case GOAPStatus.Fail:
                default:
                    SwitchStack();
                    break;
            }
        }

        private void SwitchStack()
        {
            Stack<IGOAPAction> actionStack = new Stack<IGOAPAction>();
            IGOAPAction bestSuccessor = default;
            int loop = 1000;

            while (loop-- > 0 && (bestSuccessor = FindSuccessor(bestSuccessor)) != default)
                actionStack.Push(bestSuccessor);

            CurrentAction?.OnEnd();
            this.actionStack = actionStack;
            CurrentAction?.OnStart();
        }

        // REMEMBER: This is building a path in reverse. successor -> predecessor
        private IGOAPAction FindSuccessor(IGOAPAction successor = default)
        {
            return successor != default && successor.IsFinal ? default
                : (from predecessor in actions
                   let priority = predecessor.Priority(successor)
                   where priority != null || priority != float.NaN
                   orderby priority descending
                   select predecessor).FirstOrDefault();
        }
    }

    public interface IGOAPAction
    {
        string Name { get; }
        bool IsFinal { get; }
        void OnStart();
        void OnEnd();
        GOAPStatus Update();
        float? Priority(IGOAPAction successor);
    }

    public class GOAPAction : IGOAPAction
    {
        public string Name { get; }
        public bool IsFinal { get; }
        private readonly Action onStart;
        private readonly Action onEnd;
        private readonly Func<GOAPStatus> update;
        private readonly Func<IGOAPAction, float?> priority;

        public GOAPAction(string name,
            bool isFinal = false,
            Func<IGOAPAction, float?> priority = null,
            Action onStart = null,
            Func<GOAPStatus> update = null,
            Action onEnd = null)
        {
            this.Name = name;
            this.IsFinal = isFinal;
            this.priority = priority;
            this.onStart = onStart;
            this.update = update;
            this.onEnd = onEnd;
        }

        public GOAPStatus Update() => update?.Invoke() ?? GOAPStatus.Continue;
        public float? Priority(IGOAPAction successor) => priority?.Invoke(successor) ?? null;
        public void OnStart() => onStart?.Invoke();
        public void OnEnd() => onEnd?.Invoke();
    }
}
