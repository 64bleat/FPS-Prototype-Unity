using UnityEngine;
using MPCore;

namespace MPGUI
{
    [RequireComponent(typeof(IntButton))]
    public class BotmatchInfoGUI : MonoBehaviour
    {
        public GameSelectModel botmatchInfo;

        private void Awake()
        {
            if (TryGetComponent(out IntButton button))
            {
                button.SetLabel("Bot Count");
                button.SetValue(botmatchInfo.botCount);
            }
        }

        public void SetBotCount(int value)
        {
            botmatchInfo.botCount = value;
        }
    }
}
