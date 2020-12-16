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
        private Character character;
        private Transform weaponHand;
        private GameObject currentWeaponGO;
        private int currentWeaponSlot;

        private void Awake()
        {
            character = GetComponentInParent<Character>();
            weaponHand = GetComponentInParent<CharacterBody>().rightHand;

            if (TryGetComponent(out InputManager input))
            {
                input.Bind("WeaponSlot1", () => SelectWeapon(1), this);
                input.Bind("WeaponSlot2", () => SelectWeapon(2), this);
                input.Bind("WeaponSlot3", () => SelectWeapon(3), this);
                input.Bind("WeaponSlot4", () => SelectWeapon(4), this);
                input.Bind("WeaponSlot5", () => SelectWeapon(5), this);
                input.Bind("WeaponSlot6", () => SelectWeapon(6), this);
                input.Bind("WeaponSlot7", () => SelectWeapon(7), this);
                input.Bind("WeaponSlot8", () => SelectWeapon(8), this);
                input.Bind("WeaponSlot9", () => SelectWeapon(9), this);
                input.Bind("WeaponSlot0", () => SelectWeapon(0), this);
            }

            if (drawOnStart)
                SelectBestWeapon();
        }

        public void SelectWeapon(int slot)
        { 
            if (currentWeaponSlot != slot)
            {
                Inventory nextWeapon = null;

                for (int i = 0; !nextWeapon && i < character.inventory.Count; i++)
                    if (character.inventory[i] is IWeapon w && w.WeaponSlot == slot)
                        nextWeapon = character.inventory[i];

                if(nextWeapon)
                    DrawWeapon(nextWeapon);
            }
        }

        public void SelectBestWeapon(Inventory drop = null)
        {
            Inventory nextWeapon = null;

            for (int i = 0, ie = character.inventory.Count; !nextWeapon && i < ie; i++)
                if (character.inventory[i] is IWeapon)
                    if(!drop || !character.inventory[i].Equals(drop))
                        nextWeapon = character.inventory[i];

            DrawWeapon(nextWeapon);
        }

        public void DrawWeapon(Inventory nextWeapon)
        {
            if (heldWeapon)
                if (!heldWeapon.Equals(nextWeapon))
                    Destroy(currentWeaponGO);
                else
                    return;

            if (nextWeapon)
            {
                currentWeaponGO = Instantiate((nextWeapon as IWeapon).WeaponEquip, weaponHand, false);
                currentWeaponGO.GetComponent<WeaponEquip>().weapon = (Weapon)nextWeapon;
                currentWeaponSlot = (nextWeapon as IWeapon).WeaponSlot;
            }
            else
            {
                currentWeaponGO = null;
                currentWeaponSlot = -1;
            }

            heldWeapon = nextWeapon;
        }
    }
}