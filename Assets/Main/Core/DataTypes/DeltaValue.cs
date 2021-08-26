namespace MPCore
{
    /// <summary>
    /// Used in DavaValue listeners
    /// </summary>
    public struct DeltaValue<T>
    {
        public readonly T oldValue;
        public readonly T newValue;

        public DeltaValue(T oldValue, T newValue)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }
}
