using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary> Invokes UnityEvevent on Awake </summary>
public class AwakeEvents : MonoBehaviour
{
    public UnityEvent onAwake;

    private void Awake()
    {
        onAwake?.Invoke();
    }
}
