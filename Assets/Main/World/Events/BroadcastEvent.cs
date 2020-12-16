using UnityEngine;

public class BroadcastEvent : MonoBehaviour
{
    public ObjectBroadcaster channel;
   
    public void BroadcastGameObject(GameObject v)
    {
        channel.Broadcast(v);
    }

    public void BroadcastString(string v)
    {
        channel.Broadcast(v);
    }

    public void BroadcastInt(int v)
    {
        channel.Broadcast(v);
    }

    public void BroadcastFloat(float v)
    {
        channel.Broadcast(v);
    }
}
