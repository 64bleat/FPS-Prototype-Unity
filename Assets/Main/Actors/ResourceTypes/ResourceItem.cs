using System;
using UnityEngine;

namespace MPCore
{
    /// <summary> a resource type and value of that type </summary>
    [System.Serializable]
    public class ResourceItem
    {
        public ResourceType resourceType;
        [Tooltip("starting amount of this resource")]
        public int value = 100;
        [Tooltip("maximum possible amount of this resource")]
        public int maxValue = 100;
        /// <summary> called when the resource value changes </summary>
        public Action<GameObject, int> OnValueChange;
        /// <summary> called when the resource value reaches or passes zero </summary>
        public Action<GameObject, int> OnValueZero;
    }
}
