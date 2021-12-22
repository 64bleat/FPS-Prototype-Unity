using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
	public class AIModel : Models
	{
		public HashSet<Character> characters = new();
		public HashSet<InventoryPickup> pickups = new();

		public UnityEvent<SoundEvent> AISoundEvent = new();

		public struct SoundEvent
		{
			public readonly Component source;
			public readonly Vector3 position;
			public readonly float radius;
			public readonly float volume;
		}
	}
}
