namespace MPCore
{
    /// <summary>
    /// ScriptableObject event
    /// </summary>
    /// <typeparam name="T">invocation parameter type</typeparam>
    public class ScriptableEvent<T>
    {
        public delegate void SetValue(T o);
        private event SetValue OnStateSet;
        private T state = default;

        public void Clear()
        {
            OnStateSet = null;
            state = default;
        }

        public void Invoke(T parameter)
        {
            state = parameter;
            OnStateSet?.Invoke(parameter);
        }

        /// <summary> Add a listener callback to invoke when this ScriptableEvent is invoked. </summary>
        /// <param name="OnStateSet"></param>
        /// <param name="invokeOnAdd">call action immediately upon subscription?</param>
        public void Add(SetValue OnStateSet, bool invokeOnAdd = false)
        {
            this.OnStateSet += OnStateSet;

            if (invokeOnAdd)
                OnStateSet.Invoke(state);
        }

        /// <summary> Removes all acctions associated with owner. </summary>
        /// <param name="OnStateSet"></param>
        public void Remove(SetValue OnStateSet)
        {
            this.OnStateSet -= OnStateSet;
        }

        public T Peek()
        {
            return state;
        }
    }
}
