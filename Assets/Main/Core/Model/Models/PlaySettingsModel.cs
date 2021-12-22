using MPGame;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MPCore
{
	public class PlaySettingsModel : Models
	{
		public Case scene;
		public int botCount;
		public CharacterInfo playerProfile;
		public GameManager game;
		public List<Mutator> mutators;

		public void Launch()
		{
			SceneManager.LoadScene(scene.scene.SceneName);
		}
	}
}
