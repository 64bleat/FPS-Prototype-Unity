using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
	public class NavModel : Models
	{
		public readonly List<PathMesh> activeMeshes = new();
		public List<SpawnPoint> spawnPoints = new();
	}
}
