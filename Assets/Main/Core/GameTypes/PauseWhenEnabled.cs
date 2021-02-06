using MPConsole;
using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Switches Game State related info when a menu is enabled.
    /// </summary>
    public class PauseWhenEnabled : MonoBehaviour
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
            if (applyParentOnDisable 
                && transform.parent
                && transform.parent.TryGetComponentInParent(out PauseWhenEnabled parentSwitch))
                parentSwitch.ApplySettings();

            if (pause)
                PauseManager.Release(this);
        }

        private void ApplySettings()
        {
            if(gameObject.TryGetComponentInParent(out InputManager input))
            {
                if (inputMask.Length > 0)
                    input.Mask(inputMask);
                if (inputUnmask.Length > 0)
                    input.Unmask(inputUnmask);
            }

            Cursor.lockState = cursorMode;
            Cursor.visible = cursorVisible;

        }
    }
}
