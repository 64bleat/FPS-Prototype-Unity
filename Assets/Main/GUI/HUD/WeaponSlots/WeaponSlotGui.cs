using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    public class WeaponSlotGui : MonoBehaviour
    {
        public PlayerInventoryEvents events;
        public GameObject template;
        public WeaponSlot[] slots;

        [System.Serializable]
        public class WeaponSlot
        {
            public GameObject gameObject;
            public readonly List<Weapon> weapons = new List<Weapon>();
        }

        private void Awake()
        {
            events.OnWeaponPickup += OnWeaponPickup;
            events.OnWeaponDrop += OnWeaponDrop;
            events.OnWeaponSelect.AddListener(OnWeaponSelect);
        }

        private void Start()
        {
            for (int i = 0; i < slots.Length; i++)
                DisplayWeapon(i, null);
        }

        private void OnDestroy()
        {
            events.OnWeaponPickup -= OnWeaponPickup;
            events.OnWeaponDrop -= OnWeaponDrop;
            events.OnWeaponSelect.RemoveListener(OnWeaponSelect);
        }

        private void OnWeaponSelect(int slot)
        {
            if (template.TryGetComponentInChildren(out Image timage))
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i].gameObject.TryGetComponentInChildren(out Image image))
                        if (slot == i)
                            image.color = timage.color * 2;
                        else
                            image.color = timage.color;
        }


        private void OnWeaponPickup(Weapon weapon)
        {
            List<Weapon> list = slots[weapon.weaponSlot].weapons;

            if (!list.Contains(weapon))
                list.Add(weapon);

            DisplayWeapon(weapon.weaponSlot, list.Count != 0 ? list[0] : null);
        }

        private void OnWeaponDrop(Weapon weapon)
        {
            List<Weapon> list = slots[weapon.weaponSlot].weapons;

            if (list.Contains(weapon))
                list.Remove(weapon);

            DisplayWeapon(weapon.weaponSlot, list.Count != 0 ? list[0] : null);
        }

        private void DisplayWeapon(int slotIndex, Weapon weapon)
        {
            if(slots[slotIndex].gameObject.TryGetComponentInChildren(out TextMeshProUGUI text))
            {
                text.SetText($"{slotIndex}: {(weapon ? weapon.shortName : null)}");
                text.color = weapon ? new Color(1f, 0.5f, 0f, 1f) : Color.black;
            }
        }
    }
}
