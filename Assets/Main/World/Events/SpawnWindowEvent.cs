using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWindowEvent : MonoBehaviour
{
    public ObjectBroadcaster windowChannel;
    public string title;
    [TextArea]
    public string message;
    public float windowHeight = 128;

    public GameObject windowTemplate;

    public void SpawnWindow()
    {
        if (windowChannel)
            windowChannel.Broadcast(this);
    }
}
