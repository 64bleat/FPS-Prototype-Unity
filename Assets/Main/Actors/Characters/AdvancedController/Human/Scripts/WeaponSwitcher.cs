using UnityEngine;

namespace MPCore
{
    /// <summary> Handles drawing and holstering of weapons. </summary>
    [System.Serializable]
    public class WeaponSwitcher : MonoBehaviour
    {
        public bool drawOnStart = true;
        public Weapon currentWeapon;
        public Weapon lastWeapon;
        public RectTransform emptyCrosshair;

        private Character character;
        private CharacterBody body;
        private CharacterEventManager gui;
        private GameObject currentWeaponEquip;
        private int currentWeaponSlot;

        private void Awake()
        {
            character = GetComponentInParent<Character>();
            character.TryGetComponent(out gui);
            character.TryGetComponent(out body);

            character.OnPlayerSet += OnPlayerSet;

            if (TryGetComponent(out InputManager input))
            {
                input.Bind("LastWeapon", DrawLastWeapon, this);
                input.Bind("WeaponSlot1", () => DrawWeapon(1), this);
                input.Bind("WeaponSlot2", () => DrawWeapon(2), this);
                input.Bind("WeaponSlot3", () => DrawWeapon(3), this);
                input.Bind("WeaponSlot4", () => DrawWeapon(4), this);
                input.Bind("WeaponSlot5", () => DrawWeapon(5), this);
                input.Bind("WeaponSlot6", () => DrawWeapon(6), this);
                input.Bind("WeaponSlot7", () => DrawWeapon(7), this);
                input.Bind("WeaponSlot8", () => DrawWeapon(8), this);
                input.Bind("WeaponSlot9", () => DrawWeapon(9), this);
                input.Bind("WeaponSlot0", () => DrawWeapon(0), this);
            }
        }

        private void Start()
        {
            if (currentWeapon)
                DrawWeapon(currentWeapon);
            else if (drawOnStart)
                DrawBestWeapon();
        }

        private void OnDestroy()
        {
            if(character.isPlayer)
                gui.hud.OnSetCrosshair.Invoke(null);

            character.OnPlayerSet -= OnPlayerSet;
        }

        private void OnPlayerSet(bool isPlayer)
        {
            if(isPlayer && currentWeapon)
                gui.hud.OnSetCrosshair.Invoke(currentWeapon.crosshair);
        }

        /// <summary> Draw an automatically picked weapon </summary>
        public void DrawBestWeapon()
        {
            (Weapon weapon, float priority) next = (null, float.NegativeInfinity);

            foreach (Inventory item in character.inventory)
                if (item is Weapon weapon)
                    if (weapon.weaponSlot > next.priority)
                        next = (weapon, weapon.weaponSlot);

            DrawWeapon(next.weapon);
        }

        /// <summary> Draw an automatically picked weapon that isn't currentWeapon </summary>
        public void DrawNextBestWeapon()
        {
            (Weapon weapon, float priority) next = (null, -1);

            foreach (Inventory item in character.inventory)
                if (item is Weapon weapon && weapon != currentWeapon)
                    if (weapon.weaponSlot > next.priority)
                        next = (weapon, weapon.weaponSlot);

            DrawWeapon(next.weapon);
        }

        /// <summary> Draw the previously drawn weapon if it exists </summary>
        public void DrawLastWeapon()
        {
            if(lastWeapon && character.inventory.Contains(lastWeapon))
                DrawWeapon(lastWeapon);
        }

        /// <summary> Draws a weapon in the provided slot if one exists </summary>
        public void DrawWeapon(int slot)
        { 
            if (currentWeaponSlot != slot)
            {
                Weapon nextWeapon = null;

                foreach (Inventory item in character.inventory)
                    if (item is Weapon weapon && weapon.weaponSlot == slot)
                        nextWeapon = weapon;

                if(nextWeapon)
                    DrawWeapon(nextWeapon);
            }
        }

        /// <summary> Draw any provided weapon </summary>
        /// <remarks> If no weapon exists, currentWeapon will still be holstered. </remarks>
        public void DrawWeapon(Weapon weapon)
        {
            if (currentWeapon)
                if (weapon == currentWeapon)
                    return;
                else
                {
                    lastWeapon = currentWeapon;
                    Destroy(currentWeaponEquip);
                }

            if (weapon)
            {
                Transform weaponHand;
                switch (weapon.weaponHolder)
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

                currentWeaponEquip = Instantiate(weapon.firstPersonPrefab, weaponHand, false);
                currentWeaponEquip.GetComponent<InventoryItem>().item = weapon;
                currentWeaponSlot = weapon.weaponSlot;

                if (character.isPlayer)
                    gui.hud.OnSetCrosshair.Invoke(weapon.crosshair);
            }
            else
            {
                currentWeaponEquip = null;
                currentWeaponSlot = -1;

                if (character.isPlayer)
                    gui.hud.OnSetCrosshair.Invoke(emptyCrosshair);
            }

            currentWeapon = weapon;
        }
    }
}