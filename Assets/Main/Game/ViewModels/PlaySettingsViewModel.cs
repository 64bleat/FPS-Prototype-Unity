using MPGUI;
using UnityEngine;

namespace MPCore
{
    public class PlaySettingsViewModel : MonoBehaviour
    {
        [SerializeField] IntField _botCount;

        PlaySettingsModel _settings;
        
        private void Awake()
        {
            _settings = Models.GetModel<PlaySettingsModel>();

            _botCount.SetReference(_settings, nameof(_settings.botCount));
        }
    }
}
