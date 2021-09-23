using MPGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class GUIModel : Models
    {
        public Dropdown dropdown;
        public DataValue<WindowStyle> style = new();
        public DataValue<RectTransform> crosshair = new();
        public DataValue<string> shortMessage = new();
        public DataValue<string> largeMessage = new();
        public DataValue<float> timer = new();
        public DataValue<float> speed = new();
        public DataValue<int> health = new();
        public DataValue<MessageEventParameters> killMessage = new();
        public DataValue<Weapon> currentWeapon = new();
        public DataValue<Inventory> currentPassive = new();

        public UnityEvent<Window> OpenWindow = new();
        public UnityEvent<Weapon> WeaponPickup = new();
        public UnityEvent<Weapon> WeaponDrop = new();
        public UnityEvent<Weapon> WeaponSelect = new();
        public UnityEvent<Inventory> PassivePickup = new();
        public UnityEvent<Inventory> PassiveDrop = new();
        public UnityEvent<Inventory> PassiveToggle = new();
    }

    [System.Serializable]
    public struct MessageEventParameters
    {
        public string message;
        public Color color;
        public Color bgColor;
    }
}
