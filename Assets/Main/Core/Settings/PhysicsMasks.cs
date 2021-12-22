using UnityEngine;

namespace MPCore
{
	public static class PhysicsMasks
	{
		public const string Default = nameof(Default);
		public const string Physical = nameof(Physical);
		public const string Player = nameof(Player);
		public const string Projectile = nameof(Projectile);
		public const string Interactable = nameof(Interactable);
		public static int MaskInteration;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Init()
		{
			MaskInteration = LayerMask.GetMask(Default, Physical, Player, Interactable);
		}
	}
}
