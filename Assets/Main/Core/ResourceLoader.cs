using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace MPCore
{
    public static class ResourceLoader
    {
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
