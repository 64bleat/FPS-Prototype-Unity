using UnityEngine;
using UnityEngine.Events;

/// <summary> 
/// Invokes UnityEvevent on Awake
/// </summary>
public class AwakeEvents : MonoBehaviour
{
	public UnityEvent onAwake;

	void Awake()
	{
		onAwake?.Invoke();
	}
}
