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
        public Inventory heldWeapon;
        public RectTransform emptyCrosshair;


        private Character character;
        private Transform weaponHand;
        private GameObject currentWeaponGO;
        private int currentWeaponSlot;
        private CharacterEventManager events;

        private void Awake()
        {
            character = GetComponentInParent<Character>();
            weaponHand = GetComponentInParent<CharacterBody>().rightHand;
            character.TryGetComponent(out events);

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
            if (drawOnStart)
                SelectBestWeapon();
        }

        public void SelectBestWeapon(Inventory ignoreDropped = null)
        {
            Weapon nextWeapon = null;

            for (int i = 0, ie = character.inventory.Count; !nextWeapon && i < ie; i++)
                if (character.inventory[i] is Weapon w)
                    if (!ignoreDropped || !character.inventory[i].Equals(ignoreDropped))
                        nextWeapon = w;

            DrawWeapon(nextWeapon);
        }

        public void GetWeapon(int slot)
        { 
            if (currentWeaponSlot != slot)
            {
                Weapon nextWeapon = null;

                for (int i = 0; !nextWeapon && i < character.inventory.Count; i++)
                    if (character.inventory[i] is Weapon w && w.WeaponSlot == slot)
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
                currentWeaponGO = Instantiate(nextWeapon.WeaponEquip, weaponHand, false);
                currentWeaponGO.GetComponent<WeaponEquip>().weapon = nextWeapon;
                currentWeaponSlot = nextWeapon.WeaponSlot;

                events.hud.OnSetCrosshair.Invoke(nextWeapon.crosshair);
            }
            else
            {
                currentWeaponGO = null;
                currentWeaponSlot = -1;

                events.hud.OnSetCrosshair.Invoke(emptyCrosshair);
            }

            heldWeapon = nextWeapon;
        }
    }
}