using UnityEngine;
using System.Collections.Generic;

namespace MPCore
{
    public class KeyBindList : ScriptableObject
    {
        public ScriptFloat sensitivity;
        public bool alwaysRun; 
        public List<KeyBind> keyBinds;
    }
}
