using MPConsole;
using MPCore;
using UnityEngine;

namespace MPGame
{
	/// <summary> 
	/// Root is the primary gameobject that ought to be in every scene.
	/// Singletons and global managers should be attached to the root prefab.
	/// </summary>
	public class Root : MonoBehaviour
	{
		void Awake()
		{
			Console.Reset();
		}

		void OnDestroy()
		{
			Resources.UnloadUnusedAssets();
			Models.ResetModels();
		}
	}
}
