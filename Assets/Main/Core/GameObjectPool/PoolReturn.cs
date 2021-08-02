using System;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// If the GameObject is destroyed, the pool creats another instance to maintain count
    /// </summary>
    public class PoolReturn : MonoBehaviour
    {
        [NonSerialized] public GameObjectPool parentPool;

        private void OnDestroy()
        {
            if (parentPool && (!transform.parent || !transform.parent != parentPool.transform))
                parentPool.AddInstance();
        }
    }
}
