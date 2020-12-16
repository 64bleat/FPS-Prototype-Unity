using MPCore;
using MPGUI;
using UnityEngine;

namespace MPCore
{
    public class HealthPickup : Inventory
    {
        [Header("Health")]
        public ResourceType restoreResourceType;
        public int restoreAmount = 25;
        public float percentOfMax = 0.5f;

        public override bool OnPickup(GameObject picker)
        {
            if (picker && restoreResourceType && picker.TryGetComponent(out Character character))
                foreach (ResourceItem health in character.resources)
                    if (health.resourceType == restoreResourceType)
                    {
                        int healAmount = (int)Mathf.Clamp(health.value + restoreAmount, health.value, health.maxValue * percentOfMax ) - health.value;

                        character.Heal(healAmount, picker, picker);

                        return healAmount > 0;
                    }

            return false;
        }
    }
}
