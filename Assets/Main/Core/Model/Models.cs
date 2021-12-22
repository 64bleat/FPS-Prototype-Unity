using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
	/// <summary>
	/// Models turn ScriptableObjects into static references that will not be destroyed on scene unloads
	/// or when nothing else is referencing them.
	/// </summary>
	public abstract class Models : ScriptableObject
	{
		private static readonly Dictionary<Type, Models> _instances = new Dictionary<Type, Models>();

		bool _isAsset = false;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void InitDomain()
		{
			_instances.Clear();
		}

		/// <summary>
		/// Get the model instance, preferably from Dictionary, Resource loading, or Instantiation,
		/// in that order.
		/// </summary>
		/// <remarks>
		/// An asset instance at the path Resources/'class name' will be loaded so you can set defaults
		/// </remarks>
		public static T GetModel<T>() where T : Models
		{
			Type type = typeof(T);

			// Get from Dictionary
			if (_instances.TryGetValue(type, out Models model))
				return model as T;

			// Get from Resources or Get new Instance
			T instance = Resources.Load<T>(type.Name);

			if (instance)
				instance._isAsset = true;
			else
				instance = CreateInstance<T>();

			_instances.Add(type, instance);

			instance.Init();

			return instance;
		}

		/// <summary> 
		/// Destroyes the model reference
		/// </summary>
		public static void RemoveModel<T>()
		{
			Type type = typeof(T);

			if (_instances.TryGetValue(type, out Models instance))
			{
				//DestroyImmediate(instance);
				_instances.Remove(type);
			}
		}

		public static void ResetModels()
		{
			_instances.Clear();
		}

		/// <summary>
		/// Called immediately after model is created
		/// </summary>
		protected virtual void Init()
		{

		}
	}
}