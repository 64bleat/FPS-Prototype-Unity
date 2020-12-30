using UnityEngine;

namespace MPCore
{
    [System.Serializable]
    public class KeyBindLayer : ScriptableObject
    {
        public KeyBindLayer parent;
        public KeyBind[] binds;
    }
}
