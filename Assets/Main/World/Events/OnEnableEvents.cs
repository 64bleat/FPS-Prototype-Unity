using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableEvents : MonoBehaviour
{
    public UnityEvent events;
    public UnityEvent disableEvents;

    public void OnEnable()
    {
        events.Invoke();
    }

    public void OnDisable()
    {
        events.Invoke();
    }
}
