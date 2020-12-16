using UnityEngine;

public class HudBroadcastReceiver : MonoBehaviour
{
    public HudBroadcaster hudBroadcaster;
    public RectTransform crosshairParent;

    private void Awake()
    {
        hudBroadcaster.OnSetCrosshair.Subscribe(SetCrosshair);
    }

    private void OnDestroy()
    {
        hudBroadcaster.OnSetCrosshair.Unsubscribe(SetCrosshair);
    }

    private void SetCrosshair(RectTransform crosshair)
    {
        if (hudBroadcaster.currentCrosshair)
        {
            Destroy(hudBroadcaster.currentCrosshair.gameObject);
            hudBroadcaster.currentCrosshair = null;
        }

        if(crosshair)
        {
            hudBroadcaster.currentCrosshair = Instantiate(crosshair.gameObject, crosshairParent).transform as RectTransform;
        }
    }
}
