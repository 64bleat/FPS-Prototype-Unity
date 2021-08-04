using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MPCore
{
    public class GameSelectModel : Models
    {
        public Case scene;
        public int botCount;
        public GameController game;
        public List<Case> sceneList;
        public List<CharacterInfo> botList;
        public List<GameController> gameList;

        public void Launch()
        {
            SceneManager.LoadScene(scene.scene);
        }
    }
}
