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
        public PlayerInventoryEvents events;

        private InventoryContainer container;
        private Character character;
        private CharacterBody body;
        private CharacterEventManager gui;
        private GameObject currentWeaponEquip;
        private int currentWeaponSlot;
        private Inventory selected;

        private void Awake()
        {
            TryGetComponent(out container);
            character = GetComponentInParent<Character>();
            character.TryGetComponent(out gui);
            character.TryGetComponent(out body);

            character.OnRegistered += OnPlayerSet;

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
                input.Bind("ItemActivate", ToggleSelectedActivatable, this);
                input.Bind("ItemNext", NextActivatable, this);
                input.Bind("ItemPrevious", PreviousActivatable, this);
                input.OnMouseScrollVertical += DrawScroll;
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

            character.OnRegistered -= OnPlayerSet;
        }

        private void OnPlayerSet(bool isPlayer)
        {
            if(isPlayer && currentWeapon)
                gui.hud.OnSetCrosshair.Invoke(currentWeapon.crosshair);
        }

        public void NextActivatable()
        {
            SelectActivatable(true);
        }

        private void PreviousActivatable()
        {
            SelectActivatable(false);
        }

        private void SelectActivatable(bool forward)
        {
            int count = container.inventory.Count;
            int start;
            Inventory next = null;

            if (selected)
                start = container.inventory.IndexOf(selected) + (forward ? 1 : -1);
            else
                start = 0;

            for (int i = 0; i < count; i++)
            {
                Inventory item;

                if (forward)
                    item = container.inventory[(start + i) % count];
                else
                    item = container.inventory[(start + count - i) % count];

                if (item.activatable)
                {
                    next = item;
                    break;
                }
            }

            if (selected && character.isPlayer)
                events.OnActivatableDeselect?.Invoke(selected);

            if (next)
            {
                selected = next;

                if(character.isPlayer)
                    events.OnActivatableSelect?.Invoke(next);
            }
        }

        public void ToggleSelectedActivatable()
        {
            if (selected)
            {
                selected.SetActive(character.gameObject, !selected.active);

                if (character.isPlayer)
                    events.OnActivatableToggle?.Invoke(selected);
            }
        }

        private void DrawScroll(float scroll)
        {
            int count = container.inventory.Count;
            Weapon next = null;
            Weapon loop = null;

            for (int i = 0; i < count; i++)
                if (container.inventory[i] is Weapon w)
                    if (scroll > 0)
                    {
                        if (w.weaponSlot > currentWeapon.weaponSlot && (!next || w.weaponSlot < next.weaponSlot))
                            next = w;
                        else if (w.weaponSlot < currentWeapon.weaponSlot && (!loop || w.weaponSlot < loop.weaponSlot))
                            loop = w;
                    }
                    else if (scroll < 0)
                    {
                        if (w.weaponSlot < currentWeapon.weaponSlot && (!next || w.weaponSlot > next.weaponSlot))
                            next = w;
                        else if (w.weaponSlot > currentWeapon.weaponSlot && (!loop || w.weaponSlot > loop.weaponSlot))
                            loop = w;
                    }

            if (!next)
                next = loop;

            if (next)
                DrawWeapon(next);
        }

        /// <summary> Draw an automatically picked weapon </summary>
        public void DrawBestWeapon()
        {
            (Weapon weapon, float priority) next = (null, float.NegativeInfinity);

            foreach (Inventory item in container.inventory)
                if (item is Weapon weapon)
                    if (weapon.weaponSlot > next.priority)
                        next = (weapon, weapon.weaponSlot);

            DrawWeapon(next.weapon);
        }

        /// <summary> Draw an automatically picked weapon that isn't currentWeapon </summary>
        public void DrawNextBestWeapon()
        {
            (Weapon weapon, float priority) next = (null, -1);

            foreach (Inventory item in container.inventory)
                if (item is Weapon weapon && weapon != currentWeapon)
                    if (weapon.weaponSlot > next.priority)
                        next = (weapon, weapon.weaponSlot);

            DrawWeapon(next.weapon);
        }

        /// <summary> Draw the previously drawn weapon if it exists </summary>
        public void DrawLastWeapon()
        {
            if(lastWeapon && container.inventory.Contains(lastWeapon))
                DrawWeapon(lastWeapon);
        }

        /// <summary> Draws a weapon in the provided slot if one exists </summary>
        public void DrawWeapon(int slot)
        { 
            if (currentWeaponSlot != slot)
            {
                Weapon nextWeapon = null;

                foreach (Inventory item in container.inventory)
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

                if(currentWeaponEquip.TryGetComponent(out InventoryItem ii))
                    ii.item = weapon;

                if (currentWeaponEquip.TryGetComponent(out RocketLauncherEquip rle))
                    rle.weapon = weapon;

                currentWeaponSlot = weapon.weaponSlot;

                if (character.isPlayer)
                {
                    gui.hud.OnSetCrosshair.Invoke(weapon.crosshair);
                    events.OnWeaponSelect?.Invoke(weapon.weaponSlot);
                }
            }
            else
            {
                currentWeaponEquip = null;
                currentWeaponSlot = -1;

                if (character.isPlayer)
                {
                    gui.hud.OnSetCrosshair.Invoke(emptyCrosshair);
                    events.OnWeaponSelect?.Invoke(-1);
                }
            }

            currentWeapon = weapon;
        }
    }
}