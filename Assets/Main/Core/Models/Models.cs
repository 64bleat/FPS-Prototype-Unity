using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public abstract class Models : ScriptableObject
    {
        private static readonly Dictionary<Type, Models> instances = new Dictionary<Type, Models>();

        public static T GetModel<T>() where T : Models
        {
            Type type = typeof(T);

            // Get from Dictionary
            if (instances.TryGetValue(type, out Models model))
                return model as T;

            // Get from Resources
            // Get new Instance
            T instance = Resources.Load<T>(type.Name) ?? CreateInstance<T>();

            instances.Add(type, instance);

            return instance;
        }

        public static void RemoveModel<T>()
        {
            Type type = typeof(T);

            if (instances.TryGetValue(type, out Models instance))
            {
                Destroy(instance);
                instances.Remove(type);
            }
        }
    }
}
