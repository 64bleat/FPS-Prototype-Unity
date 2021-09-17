using UnityEngine;

namespace MPCore
{
    public class HealthPickup : Inventory
    {
        [SerializeField] int _restoreAmount = 25;

        public override bool TryPickup(GameObject pickedBy)
        {
            if (pickedBy && pickedBy.TryGetComponent(out Character character))
            {
                DeltaValue<int> heal = character.Heal(_restoreAmount, character.gameObject, character.Info, pickedBy);

                return heal.oldValue != heal.newValue;
            }

            return false;
        }
    }
}
