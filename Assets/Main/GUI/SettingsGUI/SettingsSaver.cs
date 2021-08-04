using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class SettingsSaver : MonoBehaviour
    {
        private void OnDisable()
        {
            if (transform.TryGetComponentInParent(out SettingsManager settings))
                settings.SaveSettings();
        }
    }
}
