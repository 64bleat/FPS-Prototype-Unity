using UnityEngine;

namespace MPGUI
{
    public class WindowStyle : ScriptableObject
    {
        [SerializeField] internal Sprite activeTitleBackground;
        [SerializeField] internal Color activeTextColor;
        [SerializeField] internal Sprite inactiveTitleBackground;
        [SerializeField] internal Color inactiveTextColor;
    }
}
