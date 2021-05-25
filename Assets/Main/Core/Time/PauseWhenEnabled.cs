using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Pauses the game while this GameObject is enabled
    /// </summary>
    public class PauseWhenEnabled : MonoBehaviour
    {
        private void OnEnable()
        {
            PauseManager.Push(this);
        }

        private void OnDisable()
        {
            PauseManager.Pull(this);
        }
    }
}
