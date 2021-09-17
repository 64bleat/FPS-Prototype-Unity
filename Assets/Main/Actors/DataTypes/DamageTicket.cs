using UnityEngine;

namespace MPCore
{
	public struct DamageTicket
	{
		public int deltaValue;
		public DamageType damageType;
		public CharacterInfo instigator;
		public CharacterInfo victim;
		public GameObject instigatorBody;
		public GameObject victimBody;
		public Vector3 momentum;
		public Vector3 point;
		public Vector3 normal;
		public float timeStamp;
	}
}
