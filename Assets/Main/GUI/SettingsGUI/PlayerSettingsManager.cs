using MPCore;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class PlayerSettingsManager : ScriptableObject
    {
        public CharacterInfo characterInfo;
        public MPCore.Character[] availableCharacters;
    }
}
