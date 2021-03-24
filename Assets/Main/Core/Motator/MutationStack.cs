using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class MutationStack : MonoBehaviour
    {
        private readonly Stack<Mutator> stack = new Stack<Mutator>();
        private static readonly Stack<Mutator> tempStack = new Stack<Mutator>();

        public void AddMutator(Mutator mutator)
        {
            stack.Push(mutator);

            mutator.OnActivate(gameObject);
        }

        public void RemoveMutator(Mutator mutator)
        {
            while (stack.Peek() != mutator)
                tempStack.Push(stack.Pop());

            stack.Pop().OnDeactivate(gameObject);

            if (mutator.hotSwappable)
                while (tempStack.Count > 0)
                    stack.Push(tempStack.Pop());
            else
                while (tempStack.Count > 0)
                    AddMutator(tempStack.Pop());
        }
    }
}
