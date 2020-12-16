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
            if(GetComponentInParent<InputManager>() is var input && input)
                for (int i = 0, ie = binds.Length; i < ie; i++)
                {
                    UnityEvent e = binds[i].bind;
                    input.Bind(binds[i].key.name, () => e.Invoke(), this, binds[i].mode);
                }
        }
    }
}
