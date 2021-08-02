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

            if (instances.TryGetValue(type, out Models model))
                return model as T;

            Models instance = CreateInstance(type) as Models;

            instance.name = $"{type.Name}_instance";
            instances.Add(type, instance);

            return instance as T;
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
