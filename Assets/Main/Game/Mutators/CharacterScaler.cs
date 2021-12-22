using MPCore;
using UnityEngine;

namespace MPGame
{
	/// <summary>
	/// Scales character sizes
	/// </summary>
	public class CharacterScaler : Mutator
	{
		[SerializeField] float scale;

		public override void Activate()
		{
			MessageBus.Subscribe<Character>(Mutate);
			MessageBus.Subscribe<SpawnPoint>(MutateSpawn);
		}

		public override void Deactivate()
		{
			MessageBus.Unsubscribe<Character>(Mutate);
			MessageBus.Unsubscribe<SpawnPoint>(MutateSpawn);
		}

		void Mutate(Character character)
		{
			if (character.TryGetComponent(out CharacterBody bod))
				bod.ScaleBody(scale);
		}

		void MutateSpawn(SpawnPoint sp)
		{
			sp.transform.position += sp.transform.up * (scale - 1);
		}
	}
}
