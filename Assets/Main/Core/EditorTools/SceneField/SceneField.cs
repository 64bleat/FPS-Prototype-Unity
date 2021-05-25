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
        public Object _SceneAsset;
        public string _SceneName;
        public int buildIndex;

        public Object Scene => _SceneAsset;
        public string SceneName => _SceneName;

        // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }

        public static implicit operator Scene(SceneField sf)
        {
            return SceneManager.GetSceneByName(sf._SceneName);
        }

        public static implicit operator SceneField(Scene s)
        {
            SceneField sf = new SceneField();

            sf._SceneAsset = null;
            sf._SceneName = s.name;
            sf.buildIndex = s.buildIndex;

            return sf;
        }
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
