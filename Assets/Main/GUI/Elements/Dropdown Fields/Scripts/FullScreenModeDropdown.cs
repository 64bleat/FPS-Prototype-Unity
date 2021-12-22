using MPCore;
using System.Collections.Generic;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Control Screen.fullScreenMode from a dropdown
    /// </summary>
    public sealed class FullScreenModeDropdown : GenericDropdownField<FullScreenMode>
    {
        private static readonly Dictionary<FullScreenMode, string> alias = new Dictionary<FullScreenMode, string>()
        {
            {FullScreenMode.ExclusiveFullScreen, "Fullscreen" },
            {FullScreenMode.FullScreenWindow, "Borderless" },
            {FullScreenMode.MaximizedWindow, "Maximized" },
            {FullScreenMode.Windowed, "Windowed" }
        };

        public FullScreenMode Current
        {
            get => Screen.fullScreenMode;
            set => Screen.fullScreenMode = value;
        }

        void Awake()
        {
            SetReference(this, nameof(Current), "Screen Mode");
            AddOptions(alias.Keys);
        }

        protected override string Write(FullScreenMode value)
        {
            return alias[value];
        }
    }
}
