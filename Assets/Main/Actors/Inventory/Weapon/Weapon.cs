using UnityEngine;

namespace MPCore
{
    public class Weapon : Inventory
    {
        public enum WeaponHolder { RightHand, LeftHand, Center, Camera}

        [Header("Weapon")]
        public RectTransform crosshair;
        public GameObject firstPersonPrefab;
        public int weaponSlot = 1;
        public WeaponHolder weaponHolder = WeaponHolder.RightHand;
        public GameObject projectilePrimary;
        public float refireRatePrimary = 0.65f;
        [Header("WeaponAI")]
        public float preferredCombatDistance = 10f;
        public float engagementRange = 100f;
        public float validFireAngle = 5;

        public override bool OnDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            if(owner.TryGetComponentInChildren(out WeaponSwitcher ws))
                if (Equals(ws.heldWeapon))
                    ws.SelectBestWeapon(this);

            return base.OnDrop(owner, position, rotation);
        }

        public override bool OnPickup(GameObject owner)
        {
            bool baseGood = base.OnPickup(owner);

            if(baseGood)
                if(owner.TryGetComponentInChildren(out WeaponSwitcher ws))
                    if (!ws.heldWeapon)
                        ws.DrawWeapon(this);

            return baseGood;
        }
    }
}
