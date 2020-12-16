using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public enum KeyState { Up, Down, UpHold, DownHold}

    public class KeyBind : ScriptableObject
    {
        public string help;
        public KeyBindLayer layer;
        public KeyCode[] keyCombo;

        public string GetComboString()
        {
            string s = "";

            if (keyCombo.Length == 0)
                return "NULL";
            else
                for(int i = 0; i < keyCombo.Length; i++)
                {
                    s += keyCombo[i].ToString();

                    if (i < keyCombo.Length - 1)
                        s += " + ";
                }

            return s;
        }
    }
}
