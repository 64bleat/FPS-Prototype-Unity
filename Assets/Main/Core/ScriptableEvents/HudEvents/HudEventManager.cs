using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class HudEventManager : MonoBehaviour
    {
        public HudEvents hudBroadcaster;
        public RectTransform crosshairParent;

        private void Awake()
        {
            hudBroadcaster.OnSetCrosshair.Add(SetCrosshair);
        }

        private void OnDestroy()
        {
            hudBroadcaster.OnSetCrosshair.Remove(SetCrosshair);
        }

        private void SetCrosshair(RectTransform crosshair)
        {
            if (hudBroadcaster.currentCrosshair)
            {
                Destroy(hudBroadcaster.currentCrosshair.gameObject);
                hudBroadcaster.currentCrosshair = null;
            }

            if (crosshair)
            {
                hudBroadcaster.currentCrosshair = Instantiate(crosshair.gameObject, crosshairParent).transform as RectTransform;
            }
        }
    }
}
