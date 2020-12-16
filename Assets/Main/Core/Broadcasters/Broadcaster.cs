public class Broadcaster<T>
{
    public delegate void SetValue(T o);
    private event SetValue OnValueChange;
    private T lastBroadcast = default;

    public void Clear()
    {
        OnValueChange = null;
        lastBroadcast = default;
    }

    /// <summary> Broadcast a value to the listeners. </summary>
    /// <remarks> the value can be anything, so remember to type-check the listeners.</remarks>
    /// <param name="v">the value to broadcast</param>
    public void Broadcast(T v)
    {
        //if (lastBroadcast == null || v == null || !v.Equals(lastBroadcast))
        {
            lastBroadcast = v;
            OnValueChange?.Invoke(v);
        }
    }

    /// <summary> Add a listener to this channel. </summary>
    /// <param name="owner">when the owner is null, the actions are invalid and not called. </param>
    /// <param name="OnValueChange"></param>
    /// <param name="initializeImmediately">call action immediately upon subscription?</param>
    public void Subscribe(SetValue OnValueChange, bool initializeImmediately = false)
    {
        this.OnValueChange += OnValueChange;

        if (initializeImmediately)
            OnValueChange.Invoke(lastBroadcast);
    }

    /// <summary> Removes all acctions associated with owner. </summary>
    /// <param name="action"></param>
    public void Unsubscribe(SetValue action)
    {
        OnValueChange -= action;
    }
}
