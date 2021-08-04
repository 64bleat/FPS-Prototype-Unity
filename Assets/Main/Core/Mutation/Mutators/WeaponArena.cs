using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class WeaponArena : GameMutator
    {
        [SerializeField] private InventoryPickup weapon;
        public override void Mutate(GameObject gameObject)
        {
            if(gameObject.TryGetComponent(out Respawner res)
                && res.itemToSpawn.TryGetComponent(out InventoryPickup item)
                && item.inventory is Weapon)
            {
                res.itemToSpawn = weapon.gameObject;
            }
        }

        public override void MutateGame(GameObject game)
        {
            if (game.TryGetComponent(out GameController g))
            {
                g.spawnInventory.RemoveAll(i => i is Weapon);
                g.randomSpawnInventory.RemoveAll(i => i is Weapon);
                g.spawnInventory.Add(weapon.inventory);
            }
        }
    }
}
