using MPCore;
using UnityEngine;
using UnityEngine.Events;

public class ObjectBroadcastReceiver : MonoBehaviour
{
    public ObjectBroadcaster channel;
    public UnityEvent events;

    private void Awake()
    {
        channel.Subscribe(OnBroadcastReceive);
    }

    private void OnDestroy()
    {
        channel.Unsubscribe(OnBroadcastReceive);
    }

    private void OnBroadcastReceive(object o)
    {
        events?.Invoke();
    }
}
