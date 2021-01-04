using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    /// <summary> Invokes UnityEvevent OnDisable </summary>
    public class OnDisableEvents : MonoBehaviour
    {
        public UnityEvent onDisable;

        private void OnDisable()
        {
            onDisable?.Invoke();
        }
    }
}
