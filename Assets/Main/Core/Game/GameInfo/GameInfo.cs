using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MPCore
{
    public class GameInfo : ScriptableObject
    {
        public SceneInfo map;
        public int botCount;
        public Game game;
        [FormerlySerializedAs("selectableScenes")]
        public List<SceneInfo> sceneList;
        [FormerlySerializedAs("botRoster")]
        public List<CharacterInfo> botList;
        public List<Game> gameList;


        public void Play()
        {
            SceneManager.LoadScene(map.scene);
        }
    }
}
