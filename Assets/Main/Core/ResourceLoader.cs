using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace MPCore
{

	public static class ResourceLoader
	{
		/// <summary>
		/// Loads all resources of a given type within a filder.
		/// Supports finding Components.
		/// </summary>
		/// <remarks>
		/// Resource folder name defaults to <c>typeof(T).Name</c>
		/// </remarks>
		public static T[] GetResources<T>(string path = null) where T: Object 
		{
			Type type = typeof(T);
			bool isComponent = type.IsSubclassOf(typeof(Component));
			Type loadType = isComponent ? typeof(GameObject) : type;
			path ??= type.Name;
			Object[] raw = Resources.LoadAll(path, loadType);

			if (isComponent)
				return Array.ConvertAll(raw, obj => obj as GameObject)
					.Where(go => go != null)
					.Select(go => go.GetComponent<T>())
					.Where(comp => comp != null)
					.ToArray();
			else
				return Array.ConvertAll(raw, obj => obj as T);
		} 
	}
}
