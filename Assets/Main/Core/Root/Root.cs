using UnityEngine;
using MPCore;
using MPConsole;

namespace MPCore
{
    /// <summary> 
    /// Handles a few static classes
    /// </summary>
    public class Root : MonoBehaviour
    {
        public void Awake()
        {
            Console.Reset();
            PauseManager.Add(GameTime.OnPauseUnPause);
            GameTime.InitTime();
        }

        public void OnDestroy()
        {
            Resources.UnloadUnusedAssets();
            PauseManager.Remove(GameTime.OnPauseUnPause);
            PauseManager.Reset();
        }
    }
}
