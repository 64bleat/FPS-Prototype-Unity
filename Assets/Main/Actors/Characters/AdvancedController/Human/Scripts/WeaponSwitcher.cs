using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Handles switching of weapons. Searches through character inventory for weapons.
    /// </summary>
    [System.Serializable]
    public class WeaponSwitcher : MonoBehaviour
    {
        public bool drawOnStart = true;
        public Weapon heldWeapon;
        public RectTransform emptyCrosshair;

        private Character character;
        private CharacterBody body;
        private GameObject currentWeaponGO;
        private int currentWeaponSlot;
        private CharacterEventManager events;

        private void Awake()
        {
            character = GetComponentInParent<Character>();
            character.TryGetComponent(out events);
            character.TryGetComponent(out body);

            if (TryGetComponent(out InputManager input))
            {
                input.Bind("WeaponSlot1", () => GetWeapon(1), this);
                input.Bind("WeaponSlot2", () => GetWeapon(2), this);
                input.Bind("WeaponSlot3", () => GetWeapon(3), this);
                input.Bind("WeaponSlot4", () => GetWeapon(4), this);
                input.Bind("WeaponSlot5", () => GetWeapon(5), this);
                input.Bind("WeaponSlot6", () => GetWeapon(6), this);
                input.Bind("WeaponSlot7", () => GetWeapon(7), this);
                input.Bind("WeaponSlot8", () => GetWeapon(8), this);
                input.Bind("WeaponSlot9", () => GetWeapon(9), this);
                input.Bind("WeaponSlot0", () => GetWeapon(0), this);
            }
        }

        private void Start()
        {
            if (heldWeapon)
                DrawWeapon(heldWeapon);
            else if (drawOnStart)
                SelectBestWeapon();
        }

        private void OnDestroy()
        {
            if(character.isPlayer)
                events.hud.OnSetCrosshair.Invoke(null);
        }

        public void SelectBestWeapon(Inventory ignoreDropped = null)
        {
            Weapon nextWeapon = null;

            for (int i = 0, ie = character.inventory.Count; !nextWeapon && i < ie; i++)
                if (character.inventory[i] is Weapon w)
                    if (!ignoreDropped || !character.inventory[i].Equals(ignoreDropped))
                        if (!nextWeapon || nextWeapon.weaponSlot > w.weaponSlot)
                            nextWeapon = w;

            DrawWeapon(nextWeapon);
        }

        public void GetWeapon(int slot)
        { 
            if (currentWeaponSlot != slot)
            {
                Weapon nextWeapon = null;

                for (int i = 0; !nextWeapon && i < character.inventory.Count; i++)
                    if (character.inventory[i] is Weapon w && w.weaponSlot == slot)
                        nextWeapon = w;

                if(nextWeapon)
                    DrawWeapon(nextWeapon);
            }
        }
        /// <summary>
        /// Holsters the current weapon and draws a new weapon.
        /// </summary>
        /// <param name="nextWeapon">
        /// The weapon to be drawn.
        /// <para><c>Null</c> will simply holster the current weapon.</para>
        /// <para>The weapon does not necessarily have to be in the character's inventory.</para> </param>
        public void DrawWeapon(Weapon nextWeapon)
        {
            if (heldWeapon)
                if (heldWeapon.Equals(nextWeapon))
                    return;
                else 
                    Destroy(currentWeaponGO);

            if (nextWeapon)
            {
                Transform weaponHand;
                switch (nextWeapon.weaponHolder)
                {
                    case Weapon.WeaponHolder.RightHand:
                        weaponHand = body.rightHand;
                        break;
                    case Weapon.WeaponHolder.LeftHand:
                        weaponHand = body.leftHand;
                        break;
                    case Weapon.WeaponHolder.Center:
                        weaponHand = body.cameraHand;
                        break;
                    case Weapon.WeaponHolder.Camera:
                        weaponHand = body.cameraHand;
                        break;
                    default:
                        weaponHand = body.rightHand;
                        break;
                }

                currentWeaponGO = Instantiate(nextWeapon.firstPersonPrefab, weaponHand, false);
                currentWeaponGO.GetComponent<InventoryItem>().item = nextWeapon;
                currentWeaponSlot = nextWeapon.weaponSlot;

                if (character.isPlayer)
                    events.hud.OnSetCrosshair.Invoke(nextWeapon.crosshair);
            }
            else
            {
                currentWeaponGO = null;
                currentWeaponSlot = -1;

                if (character.isPlayer)
                    events.hud.OnSetCrosshair.Invoke(emptyCrosshair);
            }

            heldWeapon = nextWeapon;
        }
    }
}