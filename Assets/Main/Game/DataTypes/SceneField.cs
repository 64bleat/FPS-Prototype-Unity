using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MPCore
{
    [System.Serializable]
    public class SceneField
    {
        [SerializeField] Object _SceneAsset;
        [SerializeField] string _SceneName;
        int _buildIndex;

        public SceneField(Scene scene)
        {
            _SceneAsset = null;
            _SceneName = scene.name;
            _buildIndex = scene.buildIndex;
        }

        public Scene Scene => SceneManager.GetSceneByBuildIndex(_buildIndex);
        public string SceneName => _SceneName;
        public override string ToString() => _SceneName;

        public static implicit operator int(SceneField instance) => instance._buildIndex;
        public static implicit operator Scene(SceneField instance) => instance.Scene;
        public static implicit operator SceneField(Scene scene) => new SceneField(scene);
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            SerializedProperty sceneAsset = property.FindPropertyRelative("_SceneAsset");
            SerializedProperty sceneName = property.FindPropertyRelative("_SceneName");

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if (sceneAsset != null)
            {
                sceneAsset.objectReferenceValue = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);

                if (sceneAsset.objectReferenceValue != null)
                    sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
