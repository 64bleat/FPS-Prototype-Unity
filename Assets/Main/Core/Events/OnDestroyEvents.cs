using UnityEngine;
using UnityEngine.Events;

public class OnDestroyEvents : MonoBehaviour
{
    public UnityEvent events;

    public void OnDestroy()
    {
        events.Invoke();
    }
}
