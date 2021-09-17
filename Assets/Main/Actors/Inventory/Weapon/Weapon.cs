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

        public override bool TryDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            bool dropSuccess = base.TryDrop(owner, position, rotation);

            if(dropSuccess && owner.TryGetComponentInChildren(out WeaponSwitcher ws))
            {
                if (this == ws.currentWeapon)
                    ws.DrawNextBestWeapon();

                if (owner.TryGetComponentInChildren(out Character character) && character.IsPlayer)
                {
                    GUIModel guiModel = Models.GetModel<GUIModel>();
                    guiModel.WeaponDrop?.Invoke(this);
                }
            }

            return dropSuccess;
        }

        public override bool TryPickup(GameObject owner)
        {
            bool pickupSuccess = base.TryPickup(owner);

            if (pickupSuccess)
                if (owner.TryGetComponentInChildren(out WeaponSwitcher ws))
                {
                    if (owner.TryGetComponentInChildren(out Character character) && character.IsPlayer)
                    {
                        GUIModel guiModel = Models.GetModel<GUIModel>();
                        guiModel.WeaponPickup?.Invoke(this);
                    }

                    if (!ws.currentWeapon)
                        ws.DrawWeapon(this);
                }

            return pickupSuccess;
        }
    }
}
