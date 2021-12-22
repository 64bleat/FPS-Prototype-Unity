using MPCore;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
	public enum SurfaceFlags { Stairs, NeverFloor, InstantDeath, NoCollisionDamage, NoInfluence}

	[DisallowMultipleComponent]
	public class SurfaceFlagObject : MonoBehaviour
	{
		public SurfaceType surfaceType;
		public List<SurfaceFlags> _SurfaceFlags;
		public List<SurfaceFlag> surfaceFlags;
	}
}
