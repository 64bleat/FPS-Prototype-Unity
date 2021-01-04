using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Store all the event references relating to a character in one place
    /// </summary>
    public class CharacterEventManager : MonoBehaviour
    {
        public HudEvents hud;
        public StringEvent onSpeedSet;
    }
}
