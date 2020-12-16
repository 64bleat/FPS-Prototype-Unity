using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudBroadcaster : ScriptableObject
{
    public readonly Broadcaster<RectTransform> OnSetCrosshair = new Broadcaster<RectTransform>();
    public RectTransform currentCrosshair;
}
