using UnityEngine;
using MPCore;

namespace MPGUI
{
    [RequireComponent(typeof(IntButton))]
    public class BotmatchInfoGUI : MonoBehaviour
    {
        private PlaySettingsModel _gameSelectModel;

        private void Awake()
        {
            _gameSelectModel = Models.GetModel<PlaySettingsModel>();

            if (TryGetComponent(out IntButton button))
            {
                button.SetLabel("Bot Count");
                button.SetValue(_gameSelectModel.botCount);
            }
        }

        public void SetBotCount(int value)
        {
            _gameSelectModel.botCount = value;
        }
    }
}
