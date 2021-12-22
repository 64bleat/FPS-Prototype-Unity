using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
	public class TimedEvents : MonoBehaviour
	{
		public float duration = 1f;
		public UnityEvent events;

		void OnEnable()
		{
			StartCoroutine(TimedInvoke());
		}

		IEnumerator TimedInvoke()
		{
			yield return new WaitForSeconds(duration);

			events?.Invoke();
			enabled = false;
		}
	}
}
