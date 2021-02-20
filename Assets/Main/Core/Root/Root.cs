﻿using MPConsole;
using UnityEngine;

namespace MPCore
{
    /// <summary> 
    /// Root is the primary gameobject that ought to be in every scene.
    /// Singletons and global managers should be attached to the root prefab.
    /// </summary>
    public class Root : MonoBehaviour
    {
        public void Awake()
        {
            Console.Reset();
            PauseManager.AddListener(GameTime.OnPauseUnPause);
            GameTime.InitTime();
        }

        public void OnDestroy()
        {
            Resources.UnloadUnusedAssets();
            PauseManager.RemoveListener(GameTime.OnPauseUnPause);
            //PauseManager.Reset();
        }
    }
}
