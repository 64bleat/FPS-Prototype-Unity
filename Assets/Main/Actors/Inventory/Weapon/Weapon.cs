using UnityEngine;

namespace MPCore
{
    public class Weapon : Inventory, IWeapon
    {
        [Header("Weapon")]
        public RectTransform crosshair;
        public GameObject firstPersonPrefab;
        public int weaponSlot = 1;
        public GameObject projectilePrimary;
        public float refireRatePrimary = 0.65f;

        public GameObject WeaponEquip => firstPersonPrefab;
        public int WeaponSlot => weaponSlot;

        public override bool OnDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            if (owner.GetComponentInChildren<WeaponSwitcher>() is var ws && ws)
                if (Equals(ws.heldWeapon))
                    ws.SelectBestWeapon(this);

            return base.OnDrop(owner, position, rotation);
        }

        public override bool OnPickup(GameObject owner)
        {
            bool baseGood = base.OnPickup(owner);

            if(baseGood)
                if (owner.GetComponentInChildren<WeaponSwitcher>() is var ws && ws)
                    if (!ws.heldWeapon)
                        ws.DrawWeapon(this);

            return baseGood;
        }
    }
}
