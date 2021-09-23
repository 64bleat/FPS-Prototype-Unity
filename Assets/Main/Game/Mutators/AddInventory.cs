using UnityEngine;

namespace MPCore
{
    public class AddInventory : Mutator
    {
        [SerializeField] private Inventory item;

        public override void Activate()
        {
            Messages.Subscribe<GameController>(GiveInventory);
        }

        public override void Deactivate()
        {
            Messages.Unsubscribe<GameController>(GiveInventory);
        }

        void GiveInventory(GameController game)
        {
            game.spawnInventory.Add(item);
        }
    }
}
