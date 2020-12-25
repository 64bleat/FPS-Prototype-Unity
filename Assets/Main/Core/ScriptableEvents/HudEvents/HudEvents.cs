using UnityEngine;

namespace MPCore
{
    public class HudEvents : ScriptableObject
    {
        public readonly ScriptableEvent<RectTransform> OnSetCrosshair = new ScriptableEvent<RectTransform>();
        public RectTransform currentCrosshair;
    }
}
