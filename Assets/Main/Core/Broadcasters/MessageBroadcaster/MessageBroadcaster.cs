using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBroadcaster : ScriptableObject
{
    private readonly Broadcaster<MessageInfo> broadcaster = new Broadcaster<MessageInfo>();

    public void Clear()
    {
        broadcaster.Clear();
    }

    public void Broadcast(MessageInfo message)
    {
        broadcaster.Broadcast(message);
    }

    public void Broadcast(string message)
    {
        broadcaster.Broadcast(new MessageInfo() { message = message });
    }

    public void Subscribe(Broadcaster<MessageInfo>.SetValue OnValueChange, bool initializeImmediately = false)
    {
        broadcaster.Subscribe(OnValueChange, initializeImmediately);
    }

    public void Unsubscribe(Broadcaster<MessageInfo>.SetValue action)
    {
        broadcaster.Unsubscribe(action);
    }
}

public struct MessageInfo
{
    public string message;
}
