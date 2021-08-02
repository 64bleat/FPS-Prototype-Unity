using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public static class Singletons
    {
        private static readonly Dictionary<Guid, ScriptableObject> instances = new Dictionary<Guid, ScriptableObject>();
        public static T GetInstance<T>() where T : ScriptableObject
        {
            Type type = typeof(T);
            Guid guid = type.GUID;

            if (!instances.TryGetValue(type.GUID, out ScriptableObject instance))
            {
                instance = ScriptableObject.CreateInstance(type);
                instances.Add(guid, instance);
            }

            return instance as T;
        }
    }
}
