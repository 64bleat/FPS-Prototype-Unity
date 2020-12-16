using MPCore;
using UnityEngine;
using UnityEngine.Events;

public class VolumeEnterEvents : MonoBehaviour
{
    public UnityEvent enterEvents;
    public UnityEvent exitEvents;

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponentInChildren<Character>())
            enterEvents.Invoke();
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponentInChildren<Character>())
            exitEvents.Invoke();
    }
}
