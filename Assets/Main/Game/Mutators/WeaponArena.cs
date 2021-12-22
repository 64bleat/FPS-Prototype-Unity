using MPCore;
using UnityEngine;

namespace MPGame
{
	/// <summary>
	/// Replace Starting Weapon
	/// </summary>
	public class WeaponArena : Mutator
	{
		[SerializeField] InventoryPickup weapon;

		public override void Activate()
		{
			MessageBus.Subscribe<Respawner>(ReplaceSpawningWeapons);
			MessageBus.Subscribe<GameManager>(ReplaceStartWeapons);
		}

		public override void Deactivate()
		{
			MessageBus.Unsubscribe<Respawner>(ReplaceSpawningWeapons);
			MessageBus.Unsubscribe<GameManager>(ReplaceStartWeapons);
		}

		public void ReplaceSpawningWeapons(Respawner res)
		{
			if (res.itemPrefab.TryGetComponent(out InventoryPickup item)
				&& item.inventory is Weapon)
				res.itemPrefab = item;
		}

		public void ReplaceStartWeapons(GameManager game)
		{
			game.SpawnInventory.RemoveAll(i => i is Weapon);
			game.SpawnInventory.Add(weapon.inventory);
		}
	}
}
