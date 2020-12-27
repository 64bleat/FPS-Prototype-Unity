using MPConsole;
using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Switches Game State related info when a menu is enabled.
    /// </summary>
    public class GameStateSwitch : MonoBehaviour
    {
        public bool pause = false;
        public CursorLockMode cursorMode = CursorLockMode.Locked;
        public bool cursorVisible = false;
        public string inputMask;
        public string inputUnmask = "Ingame";
        public bool applyOnEnable = true;
        public bool applyParentOnDisable = false;

        private void OnEnable()
        {
            if(applyOnEnable)
                ApplySettings();

            if (pause)
                PauseManager.Request(this);
        }

        private void OnDisable()
        {
            if (applyParentOnDisable && transform.parent
                && transform.parent.GetComponentInParent<GameStateSwitch>() is var p && p)
                p.ApplySettings();

            if (pause)
                PauseManager.Release(this);
        }

        public void ApplySettings()
        {
            if (GetComponentInParent<InputManager>() is var input && input)
            {
                if (inputMask != null && inputMask.Length > 0)
                    input.Mask(inputMask);
                if (inputUnmask != null && inputUnmask.Length > 0)
                    input.Unmask(inputUnmask);
            }
            //Console.Paused = pause;
            Cursor.lockState = cursorMode;
            Cursor.visible = cursorVisible;

        }
    }
}
