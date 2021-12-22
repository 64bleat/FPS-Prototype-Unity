using MPCore;
using MPGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPGame
{
	public class RegenerationMutator : Mutator
	{
		[SerializeField] float _waitSeconds;
		[SerializeField] int _increment;

		public override void Activate()
		{
			MessageBus.Subscribe<Character>(BeginRegeneration);
		}

		public override void Deactivate()
		{
			MessageBus.Unsubscribe<Character>(BeginRegeneration);
		}

		void BeginRegeneration(Character character)
		{
			character.StartCoroutine(Regenerate(character));
		}

		IEnumerator Regenerate(Character character)
		{
			while(true)
			{
				yield return new WaitForSeconds(_waitSeconds);
				character.Health.Value += _increment;
			}
		}
	}
}
