using UnityEngine;

public class ObjectBroadcaster : ScriptableObject
{
    private readonly Broadcaster<object> broadcaster = new Broadcaster<object>();

    public void Clear()
    {
        broadcaster.Clear();
    }

    public void Broadcast(object o)
    {
        broadcaster.Broadcast(o);
    }

    public void Subscribe(Broadcaster<object>.SetValue OnValueChange, bool initializeImmediately = false)
    {
        broadcaster.Subscribe(OnValueChange, initializeImmediately);
    }

    public void Unsubscribe(Broadcaster<object>.SetValue action)
    {
        broadcaster.Unsubscribe(action);
    }
}