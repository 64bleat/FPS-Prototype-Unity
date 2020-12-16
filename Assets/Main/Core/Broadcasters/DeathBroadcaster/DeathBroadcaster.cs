using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBroadcaster : ScriptableObject
{
    private readonly Broadcaster<DeathEventInfo> broadcaster = new Broadcaster<DeathEventInfo>();

    public void Clear()
    {
        broadcaster.Clear();
    }

    public void Broadcast(DeathEventInfo o)
    {
        broadcaster.Broadcast(o);
    }

    public void Subscribe(Broadcaster<DeathEventInfo>.SetValue OnValueChange, bool initializeImmediately = false)
    {
        broadcaster.Subscribe(OnValueChange, initializeImmediately);
    }

    public void Unsubscribe(Broadcaster<DeathEventInfo>.SetValue action)
    {
        broadcaster.Unsubscribe(action);
    }
}
