using UnityEngine;

namespace MPCore
{
    public class WeaponArena : Mutator
    {
        [SerializeField] private InventoryPickup weapon;

        public override void Activate()
        {
            Messages.Subscribe<Respawner>(ReplaceSpawningWeapons);
            Messages.Subscribe<GameController>(ReplaceStartWeapons);
        }

        public override void Deactivate()
        {
            Messages.Unsubscribe<Respawner>(ReplaceSpawningWeapons);
            Messages.Unsubscribe<GameController>(ReplaceStartWeapons);
        }

        public void ReplaceSpawningWeapons(Respawner res)
        {
            if (res.itemToSpawn.TryGetComponent(out InventoryPickup item)
                && item.inventory is Weapon)
                res.itemToSpawn = weapon.gameObject;
        }

        public void ReplaceStartWeapons(GameController game)
        {
            game.spawnInventory.RemoveAll(i => i is Weapon);
            game.randomSpawnInventory.RemoveAll(i => i is Weapon);
            game.spawnInventory.Add(weapon.inventory);

        }
    }
}
