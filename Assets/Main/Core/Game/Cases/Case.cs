using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Describes a loadable scenario
    /// </summary>
    public class Case : ScriptableObject
    {
        public SceneField scene;
        public string displayName;
    }
}