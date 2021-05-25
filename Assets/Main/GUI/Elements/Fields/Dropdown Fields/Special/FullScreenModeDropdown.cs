using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class FullScreenModeDropdown : DropdownField
    {
        private static readonly Dictionary<FullScreenMode, string> alias = new Dictionary<FullScreenMode, string>()
        {
            {FullScreenMode.ExclusiveFullScreen, "Fullscreen" },
            {FullScreenMode.FullScreenWindow, "Borderless" },
            {FullScreenMode.MaximizedWindow, "Maximized" },
            {FullScreenMode.Windowed, "Windowed" }
        };

        protected override void InitField()
        {
            FullScreenMode fsm = Screen.fullScreenMode;

            if(!alias.TryGetValue(fsm, out string text))
                text = Screen.fullScreenMode.ToString();

            valueText.SetText(text);
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);
            FullScreenMode current = Screen.fullScreenMode;

            foreach (FullScreenMode fsm in Enum.GetValues(typeof(FullScreenMode)))
            {
                FullScreenMode mode = fsm;
                string description = alias[fsm];
                void call()
                {
                    Screen.fullScreenMode = mode;
                    InitField();
                };
                GameObject go = set.AddButton(description, call);
                Image image = go.GetComponent<Image>();

                if (image && mode != current)
                    image.color *= 0.5f;
            }
        }
    }
}
