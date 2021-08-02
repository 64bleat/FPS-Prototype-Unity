using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MPCore
{
    public class GameSelectModel : ScriptableObject
    {
        public Case scene;
        public int botCount;
        public GameInfo game;
        public List<Case> sceneList;
        public List<CharacterInfo> botList;
        public List<GameInfo> gameList;

        public void Launch()
        {
            SceneManager.LoadScene(scene.scene);
        }
    }
}
