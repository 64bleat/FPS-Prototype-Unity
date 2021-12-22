using MPCore;
using UnityEngine;

namespace MPGame
{
	/// <summary>
	/// Affect game's starting inventory
	/// </summary>
	public class AddInventory : Mutator
	{
		[SerializeField] Inventory item;

		public override void Activate()
		{
			MessageBus.Subscribe<GameManager>(GiveInventory);
		}

		public override void Deactivate()
		{
			MessageBus.Unsubscribe<GameManager>(GiveInventory);
		}

		void GiveInventory(GameManager game)
		{
			game.SpawnInventory.Add(item);
		}
	}
}
