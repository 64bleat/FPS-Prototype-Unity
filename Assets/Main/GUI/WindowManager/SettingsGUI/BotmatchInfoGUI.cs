using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPCore;

namespace MPGUI
{
    public class BotmatchInfoGUI : MonoBehaviour
    {
        public GUIIntButton botCountButton;
        public BotmatchGameInfo botmatchInfo;

        private void Awake()
        {
            botCountButton.SetValue(botmatchInfo.botCount);
            botCountButton.OnValueChange += v => botmatchInfo.botCount = v;
        }
    }
}
