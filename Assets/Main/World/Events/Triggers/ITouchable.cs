using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public interface ITouchable
    {
        void OnTouch(GameObject instigator, Collision hit);
    }
}
