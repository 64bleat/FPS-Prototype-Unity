using System;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public class WeaponSlotEvents : ScriptableObject
    {
        public event Action<Weapon> OnWeaponPickup;
        public event Action<Weapon> OnWeaponDrop;
        public UnityEvent<int> OnWeaponSelect;
        public UnityEvent<Inventory> OnActivatablePickup;
        public UnityEvent<Inventory> OnActivatableDrop;
        public UnityEvent<Inventory> OnActivatableToggle;
        public UnityEvent<Inventory> OnActivatableSelect;
        public UnityEvent<Inventory> OnActivatableDeselect;

        public void DropWeapon(Weapon weapon)
        {
            OnWeaponDrop?.Invoke(weapon);
        }

        public void PickupWeapon(Weapon weapon)
        {
            OnWeaponPickup?.Invoke(weapon);
        }
    }
}
