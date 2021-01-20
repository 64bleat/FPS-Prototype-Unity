using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class PoolReturn : MonoBehaviour
    {
        [NonSerialized] public GameObjectPool returnPool;

        private void OnDestroy()
        {
            if (returnPool && (!transform.parent || !transform.parent != returnPool.transform))
                returnPool.AddInstance();
        }
    }
}
