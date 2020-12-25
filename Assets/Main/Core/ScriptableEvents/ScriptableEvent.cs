namespace MPCore
{
    /// <summary>
    /// ScriptableObject event
    /// </summary>
    /// <typeparam name="T">invocation parameter type</typeparam>
    public class ScriptableEvent<T>
    {
        public delegate void SetValue(T o);
        private event SetValue OnValueChange;
        private T value = default;

        public void Clear()
        {
            OnValueChange = null;
            value = default;
        }

        public void Invoke(T parameter)
        {
            value = parameter;
            OnValueChange?.Invoke(parameter);
        }

        /// <summary> Add a listener to this channel. </summary>
        /// <param name="owner">when the owner is null, the actions are invalid and not called. </param>
        /// <param name="OnValueChange"></param>
        /// <param name="invokeOnAdd">call action immediately upon subscription?</param>
        public void Add(SetValue OnValueChange, bool invokeOnAdd = false)
        {
            this.OnValueChange += OnValueChange;

            if (invokeOnAdd)
                OnValueChange.Invoke(value);
        }

        /// <summary> Removes all acctions associated with owner. </summary>
        /// <param name="OnValueChange"></param>
        public void Remove(SetValue OnValueChange)
        {
            this.OnValueChange -= OnValueChange;
        }
    }
}
