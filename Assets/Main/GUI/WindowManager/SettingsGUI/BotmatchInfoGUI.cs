using UnityEngine;
using MPCore;

namespace MPGUI
{
    [RequireComponent(typeof(GUIIntButton))]
    public class BotmatchInfoGUI : MonoBehaviour
    {
        public BotmatchGameInfo botmatchInfo;

        private void Awake()
        {
            if (TryGetComponent(out GUIIntButton button))
            {
                button.valueName.SetText(botmatchInfo.botCount.ToString());
                button.description.SetText("Bot Count");
                button.value = botmatchInfo.botCount;
            }
        }

        public void SetBotCount(int value)
        {
            botmatchInfo.botCount = value;
        }
    }
}
