using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace MPCore
{
	[Serializable]
	public class SettingsData
	{
		[XmlArray("Settings"), XmlArrayItem("v")]
		public List<KVP<string, string>> values = new List<KVP<string, string>>();

		[XmlArray("Keys")][XmlArrayItem("k")]
		public List<KVP<string, KeyCode[]>> keyBinds = new List<KVP<string, KeyCode[]>>();

		readonly Dictionary<string, string> _valueMap = new Dictionary<string, string>();
		readonly Dictionary<string, KeyCode[]> _keyMap = new Dictionary<string, KeyCode[]>();
		bool _initialized = false;

		public struct KVP<T,U>
		{
			[XmlAttribute("k")] public T key;
			[XmlAttribute("v")] public U value;

			public KVP(T key, U value)
			{
				this.key = key;
				this.value = value;
			}
		}

		void Initialize()
		{
			_valueMap.Clear();
			_keyMap.Clear();

			foreach (var kvp in values)
				if (!_valueMap.ContainsKey(kvp.key))
					_valueMap.Add(kvp.key, kvp.value);
				else
					Debug.LogError($"Duplicate ValueMap loaded: {kvp.key}");

			foreach (KVP<string, KeyCode[]> kbx in this.keyBinds)
				if (!_keyMap.ContainsKey(kbx.key))
					_keyMap.Add(kbx.key, kbx.value);
				else
					Debug.LogError($"Duplicate KeyMap loaded: {kbx.key}");

			_initialized = true;
		}

		public void Clear()
		{
			values.Clear();
			keyBinds.Clear();
			_valueMap.Clear();
			_keyMap.Clear();
		}

		public void SetValue(string key, string value)
		{
			values.Add(new KVP<string, string>(key, value));
			_initialized = false;
		}

		public void SetKey(string key, KeyCode[] value)
		{
			keyBinds.Add(new KVP<string, KeyCode[]>(key, value));
			_initialized = false;
		}

		public string GetValue(string key)
		{
			if (!_initialized)
				Initialize();

			if (!_valueMap.TryGetValue(key, out string value))
				value = string.Empty;

			return value;
		}

		public bool TryGetValue(string key, out string value)
		{
			if (!_valueMap.TryGetValue(key, out value))
				value = null;

			return !string.IsNullOrWhiteSpace(value);
		}

		public KeyCode[] GetKey(string key)
		{
			if (!_initialized)
				Initialize();

			if (!_keyMap.TryGetValue(key, out KeyCode[] combo))
				combo = new KeyCode[0];

			return combo;
		}

		//FieldInfo[] keyFields = typeof(KeyModel).GetFields();
		//var keyFloats = keyFields
		//	.Where(field => field.FieldType == typeof(DataValue<float>))
		//	.Select(field => (
		//		name: field.Name,
		//		field: field.GetValue(_keyModel) as DataValue<float>));
		//foreach ((string name, DataValue<float> field) in keyFloats)
		//	if (float.TryParse(data.GetValue(name), out fValue))
		//		field.Value = fValue;
	}
}
