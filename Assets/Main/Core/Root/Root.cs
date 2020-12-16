using UnityEngine;
using MPCore;
using MPConsole;

namespace MPCore
{
    /// <summary> 
    ///     Master GameObject
    ///     
    ///     Handles various static classes that need some 
    /// </summary>
    public class Root : MonoBehaviour
    {
        public void Awake()
        {
            Console.Reset();
        }

        public void OnDestroy()
        {
            Navigator.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
