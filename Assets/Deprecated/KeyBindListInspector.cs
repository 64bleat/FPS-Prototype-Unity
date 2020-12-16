//using UnityEngine;
//using UnityEditor;

//namespace SHK.IO
//{
//    //[CustomEditor(typeof(KeyBindList))]
//    public class KeyBindListInspector : Editor
//    {
//        KeyBindList customList;
//        //SerializedObject customListSO;
//        //SerializedProperty list;

//        void OnEnable()
//        {
//            customList = (KeyBindList)target;
//            //customListSO = new SerializedObject(customList);
//            //list = customListSO.FindProperty("MyList");
//        }

//        public override void OnInspectorGUI()
//        {
//            //customListSO.Update();

//            customList.MouseSensitivity = EditorGUILayout.FloatField("Mouse Sensitivity", customList.MouseSensitivity);

//            EditorGUILayout.BeginHorizontal();
//            {
//                if (GUILayout.Button("Add New"))
//                    customList.bindList.Add(new KeyBindList.KeyBind());
//            }
//            EditorGUILayout.EndHorizontal();

//            for (int i = 0; i < customList.bindList.Count; i++)
//            {
//                KeyBindList.KeyBind node = customList.bindList[i];

//                node.name = EditorGUILayout.TextField("Name", node.name);
//                node.layer = EditorGUILayout.TextField("Layer", node.layer);
//                node.help = EditorGUILayout.TextField("Help", node.help);

//                EditorGUILayout.BeginHorizontal();
//                {
//                    EditorGUILayout.LabelField("Key Combo");

//                    if (GUILayout.Button("Add New Index", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
//                        node.keyCombo.Add(0);
//                }
//                EditorGUILayout.EndHorizontal();

//                for (int a = 0; a < node.keyCombo.Count; a++)
//                {
//                    EditorGUILayout.BeginHorizontal();
//                    {
//                        node.keyCombo[a] = (KeyCode)EditorGUILayout.EnumPopup(node.keyCombo[a]);

//                        if (GUILayout.Button("-", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
//                            node.keyCombo.RemoveAt(a);
//                    }

//                    EditorGUILayout.EndHorizontal();
//                }

//                EditorGUILayout.BeginHorizontal();
//                {
//                    if (GUILayout.Button("Remove"))
//                        customList.bindList.Remove(node);

//                    if(GUILayout.Button("Up") && i > 0)
//                    {
//                        KeyBindList.KeyBind c = customList.bindList[i];
//                        customList.bindList[i] = customList.bindList[i - 1];
//                        customList.bindList[i - 1] = c;
//                    }

//                    if (GUILayout.Button("Down") && i < customList.bindList.Count - 1)
//                    {
//                        KeyBindList.KeyBind c = customList.bindList[i];
//                        customList.bindList[i] = customList.bindList[i + 1];
//                        customList.bindList[i + 1] = c;
//                    }
//                }
//                EditorGUILayout.EndHorizontal();
//            }

//            //customListSO.ApplyModifiedProperties();
//        }
//    }
//}