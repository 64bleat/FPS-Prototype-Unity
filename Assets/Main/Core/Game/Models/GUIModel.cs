using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class GUIModel : Models
    {
        public UnityEvent<string> ShortMessage = new UnityEvent<string>();
    }
}
