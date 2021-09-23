using System;
using System.Reflection;

namespace MPCore
{
	/// <summary>
	/// A reference to a Field or Property through Reflection
	/// </summary>
	/// <typeparam name="T">Type of the Field or Property</typeparam>
	public sealed class ReflectionValue<T>
	{
		private readonly object _instance;
		private readonly FieldInfo _field;
		private readonly PropertyInfo _property;
		private readonly string _displayName;

		/// <summary>
		/// Pass an instance along with the name of a field or property that is part of that class
		/// </summary>
		public ReflectionValue(object instance, string fieldName, string displayName = null)
		{
			Type required = typeof(T);
			Type instanceType = instance.GetType();

			_instance = instance;
			_field = instanceType.GetField(fieldName);
			_property = instanceType.GetProperty(fieldName);

			if (_field?.FieldType != required)
				_field = null;
			if (_property?.PropertyType != required)
				_property = null;

			_displayName = displayName ?? fieldName;
		}

		/// <summary>
		/// Get and Sets the ReflectionValue
		/// </summary>
		public T Value
		{
			get
			{
				return (T)(_field?.GetValue(_instance) 
					?? _property.GetValue(_instance));
			}
			set
			{
				_field?.SetValue(_instance, value);
				_property?.SetValue(_instance, value);
			}
		}

		/// <summary>
		/// String description of the value
		/// </summary>
		public string DisplayName => _displayName;

		/// <summary>
		/// Implicit cast to the correct value
		/// </summary>
		public static explicit operator T(ReflectionValue<T> rv) => rv.Value;
	}
}
