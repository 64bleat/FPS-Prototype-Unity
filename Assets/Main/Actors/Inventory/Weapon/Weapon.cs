using UnityEngine;

namespace MPCore
{
    public class Weapon : Inventory
    {
        public enum WeaponHolder { RightHand, LeftHand, Center, Camera}

        [Header("Weapon")]
        public string shortName;
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
            bool basePass = base.OnDrop(owner, position, rotation);

            if(basePass && owner.TryGetComponentInChildren(out WeaponSwitcher ws))
            {
                if (this == ws.currentWeapon)
                    ws.DrawNextBestWeapon();

                if (owner.TryGetComponentInChildren(out Character character) && character.isPlayer)
                    ws.events.DropWeapon(this);
            }

            return basePass;
        }

        public override bool OnPickup(GameObject owner)
        {
            bool basePass = base.OnPickup(owner);

            if (basePass)
                if (owner.TryGetComponentInChildren(out WeaponSwitcher ws))
                {
                    if (!ws.currentWeapon)
                        ws.DrawWeapon(this);

                    if(owner.TryGetComponentInChildren(out Character character) && character.isPlayer)
                        ws.events.PickupWeapon(this);
                }

            return basePass;
        }
    }
}
