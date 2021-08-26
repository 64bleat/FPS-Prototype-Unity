using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class SettingsSaver : MonoBehaviour
    {
        private void OnDisable()
        {
            SettingsModel model = Models.GetModel<SettingsModel>();

            model.Save?.Invoke();
        }
    }
}
