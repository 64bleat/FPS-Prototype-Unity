using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class DamageType : ScriptableObject
    {
        [Header("%i = instigator %t = target %(i,t)p = pronoun")]
        public string killMessage;
        public string suicideMessage;
        public string assistedSuicideMessage;
    }
}
