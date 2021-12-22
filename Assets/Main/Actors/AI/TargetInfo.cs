using UnityEngine;

namespace MPCore.AI
{
    public struct TargetInfo
    {
        public float priority;
        public float lastSeen;
        public float firstSeen;
        public Component component;
        public Vector3 mentalPosition;
    }
}
