using MPCore;
using UnityEngine;
using UnityEngine.Events;

public class ObjectEventComponent : MonoBehaviour
{
    public ObjectEvent channel;
    public UnityEvent events;

    private void Awake()
    {
        channel.Add(Invoke);
    }

    private void OnDestroy()
    {
        channel.Remove(Invoke);
    }

    private void Invoke(object o)
    {
        events?.Invoke();
    }
}
