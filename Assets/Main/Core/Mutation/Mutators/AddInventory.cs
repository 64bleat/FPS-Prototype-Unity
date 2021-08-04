using UnityEngine;

namespace MPCore
{
    public class AddInventory : GameMutator
    {
        [SerializeField] private Inventory item;

        public override void Mutate(GameObject gameObject)
        {

        }

        public override void MutateGame(GameObject game)
        {
            if (game.TryGetComponent(out GameController g))
                g.spawnInventory.Add(item);
        }
    }
}
