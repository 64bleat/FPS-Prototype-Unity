using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    /// <summary> static manager for lists of resources </summary>
    public static class ResourceManager
    {
        /// <summary> Consume a resource by the value. </summary>
        /// <param name="list">the list of available resources</param>
        /// <param name="type">the resource type to be consumed</param>
        /// <param name="value">the amount to be consumed (negative values restore)</param>
        /// <returns> the remaining resource amount </returns>
        public static int Consume(List<ResourceValue> list, ResourceType type, int value, GameObject instigator)
        {
            foreach (ResourceValue resource in list)
                if (resource.resourceType.resourcePath.Equals(type.resourcePath))
                {
                    int oldValue = resource.value;

                    resource.value = Mathf.Min(resource.maxValue, resource.value - value);

                    if (resource.value != oldValue)
                        resource.OnValueChange?.Invoke(instigator, resource.value);

                    if (oldValue > 0 && resource.value <= 0)
                        resource.OnValueZero?.Invoke(instigator, resource.value);

                    return resource.value;
                }

            return 0;
        }

        /// <summary> get the value of a specific resource  </summary>
        /// <param name="list">the list of available resources</param>
        /// <param name="type">the resource type to be sampled</param>
        /// <returns> the current value of the resource type in the list</returns>
        public static int ValueOf(List<ResourceValue> list, ResourceType type)
        {
            foreach (ResourceValue resource in list)
               if (resource.resourceType.formalName.Equals(type.formalName))
                    return resource.value;

            return 0;
        }

        /// <summary> get the full resource class in a list </summary>
        /// <param name="list">the list of available resources</param>
        /// <param name="type">the resource type to be consumed</param>
        /// <returns> the entire resource item </returns>
        public static ResourceValue GetResource(List<ResourceValue> list, ResourceType type)
        {
            foreach (ResourceValue resource in list)
                if (resource.resourceType.formalName.Equals(type.formalName))
                    return resource;

            return null;
        }
    }

    
}