using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class SettingsSaver : MonoBehaviour
    {
        public void Save()
        {
            if (transform.TryGetComponentInParent(out SettingsManager settings))
                settings.SaveSettings();
        }
    }
}
