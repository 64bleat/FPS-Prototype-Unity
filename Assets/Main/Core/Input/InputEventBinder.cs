using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    /// <summary> Binds events to the nearest InputManager in parents </summary>
    public class InputEventBinder : MonoBehaviour
    {
        public Bind[] binds;

        [System.Serializable]
        public struct Bind
        {
            public KeyBind key;
            public KeyPressType mode;
            public UnityEvent bind;
        }

        private void OnEnable()
        {
            if(gameObject.TryGetComponentInParent(out InputManager input))
                for (int i = 0, ie = binds.Length; i < ie; i++)
                    input.Bind(binds[i].key.name, binds[i].bind.Invoke, this, binds[i].mode);
        }

        private void OnDisable()
        {
            if (gameObject.TryGetComponentInParent(out InputManager input))
                input.Unbind(this);
        }
    }
}
