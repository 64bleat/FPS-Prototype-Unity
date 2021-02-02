using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    public class PickupSlotGui : MonoBehaviour
    {
        public WeaponSlotEvents events;
        public GameObject template;

        private readonly Dictionary<Inventory, GameObject> map = new Dictionary<Inventory, GameObject>();
        private readonly Color color_inactive = Color.black;
        private readonly Color color_active = new Color(1f, 0.5f, 1f, 1f);

        public void Awake()
        {
            events.OnActivatablePickup.AddListener(OnActivatablePickup);
            events.OnActivatableDrop.AddListener(OnActivatableDrop);
            events.OnActivatableToggle.AddListener(OnActiveToggled);
            events.OnActivatableSelect.AddListener(OnActiveSelected);
            events.OnActivatableDeselect.AddListener(OnActivatableDeslected);
        }

        public void OnDestroy()
        {
            events.OnActivatablePickup.RemoveListener(OnActivatablePickup);
            events.OnActivatableDrop.RemoveListener(OnActivatableDrop);
            events.OnActivatableToggle.RemoveListener(OnActiveToggled);
            events.OnActivatableSelect.RemoveListener(OnActiveSelected);
            events.OnActivatableDeselect.RemoveListener(OnActivatableDeslected);

            foreach (GameObject obj in map.Values)
                Destroy(obj);
        }

        private void OnActivatablePickup(Inventory item)
        {
            GameObject entry = Instantiate(template, template.transform.parent);

            if (entry.TryGetComponentInChildren(out TextMeshProUGUI text))
            {
                text.SetText(item.displayName);
                text.color = item.active ? color_active : color_inactive;
            }

            map.Add(item, entry);

            entry.SetActive(true);
        }

        private void OnActivatableDrop(Inventory item)
        {
            if (map.TryGetValue(item, out GameObject entry))
                Destroy(entry);

            map.Remove(item);
        }

        private void OnActiveToggled(Inventory item)
        {
            if (map.TryGetValue(item, out GameObject entry))
                if (entry.TryGetComponentInChildren(out TextMeshProUGUI text))
                    text.color = item.active ? color_active : color_inactive;
        }

        private void OnActiveSelected(Inventory item)
        {
            if (map.TryGetValue(item, out GameObject entry))
                if (entry.TryGetComponentInChildren(out Image image))
                    if (template.TryGetComponentInChildren(out Image templateImage))
                        image.color = templateImage.color * 2;
        }

        private void OnActivatableDeslected(Inventory item)
        {
            if (map.TryGetValue(item, out GameObject entry))
                if (entry.TryGetComponentInChildren(out Image image))
                    if (template.TryGetComponentInChildren(out Image templateImage))
                        image.color = templateImage.color;
        }
    }
}
