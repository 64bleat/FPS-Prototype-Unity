using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringBroadcaster : ScriptableObject
{
    private readonly Broadcaster<string> broadcaster = new Broadcaster<string>();

    public void Clear()
    {
        broadcaster.Clear();
    }

    public void Broadcast(string o)
    {
        broadcaster.Broadcast(o);
    }

    public void Subscribe(Broadcaster<string>.SetValue OnValueChange, bool initializeImmediately = false)
    {
        broadcaster.Subscribe(OnValueChange, initializeImmediately);
    }

    public void Unsubscribe(Broadcaster<string>.SetValue action)
    {
        broadcaster.Unsubscribe(action);
    }
}
