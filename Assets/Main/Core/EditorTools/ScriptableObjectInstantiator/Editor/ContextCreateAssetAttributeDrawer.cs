/******************************************************************************
 * Thank you for making ScriptableObjects way more fun to use,  
 * now I'm gonna make it better.
 * https://forum.unity.com/threads/generic-create-scriptableobject-attribute.517847/
 *****************************************************************************/
#pragma warning disable IDE0051 // Remove unused private members

using MPCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MPEditor
{
	[CustomPropertyDrawer(typeof(ContextCreateAssetAttribute), true)]
	public class ContextCreateAssetAttributeDrawer : PropertyDrawer
	{

		[MenuItem("Assets/Instantiate ScriptableObject", true)]
		private static bool CreateWithContextMenuValidate()
		{
			MonoScript selection = Selection.activeObject as MonoScript;

			return selection ? selection.GetClass()?.IsSubclassOf(typeof(ScriptableObject)) ?? false : false;
		}

		[MenuItem("Assets/Instantiate ScriptableObject")]
		private static void CreateWithContextMenu()
		{
			MonoScript ms = Selection.activeObject as MonoScript;

			if (ms)
			{
				Type type = ms.GetClass();

				MakeAsset(MakePathFor(type), type);
			}
		}

		private static void CreateWithInspector(object o, Type type)
		{
			ScriptableObject asset = MakeAsset(MakePathFor(type), type);
			SerializedProperty prop = (o as SerializedProperty);

			prop.objectReferenceValue = asset;
			prop.serializedObject.ApplyModifiedProperties();
		}

		private static ScriptableObject MakeAsset(string path, Type type)
		{
			AssetDatabase.Refresh();
			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
			return asset;
		}

		private static string MakePathFor(Type type)
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);

			if (path.Length != 0)
				path = path.Substring(0, path.LastIndexOf('/') + 1);
			else
				path = "Assets" + Path.AltDirectorySeparatorChar;

			return AssetDatabase.GenerateUniqueAssetPath(path + type.Name + ".asset");
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property, label, true);

			if (position.Contains((Event.current.mousePosition))
				&& Event.current.type == EventType.ContextClick)
			{
				GenericMenu menu = new GenericMenu();

				if (fieldInfo.FieldType.IsSubclassOf(typeof(ScriptableObject)))
					foreach (Type t in SubclassesOf(fieldInfo.FieldType))
						menu.AddItem(new GUIContent(t.FullName.Replace('.', '/')), false, o => CreateWithInspector(o, t), property);

				menu.ShowAsContext();
			}
		}

		public static IEnumerable<Type> SubclassesOf(Type selectType)
		{
			return new List<Type>(from t in Assembly.GetAssembly(selectType).GetTypes()
									where t.IsClass && !t.IsAbstract && t.IsSubclassOf(selectType) || t == selectType
									select t);
		}
	}
}