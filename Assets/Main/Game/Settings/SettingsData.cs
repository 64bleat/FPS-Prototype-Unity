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

        private readonly Dictionary<string, string> valueMap = new Dictionary<string, string>();
        private readonly Dictionary<string, KeyCode[]> keyMap = new Dictionary<string, KeyCode[]>();
        private bool _initialized = false;

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

        public void Clear()
        {
            values.Clear();
            keyBinds.Clear();
            valueMap.Clear();
            keyMap.Clear();
        }

        private void Initialize()
        {
            valueMap.Clear();
            keyMap.Clear();

            foreach (var kvp in this.values)
                valueMap.Add(kvp.key, kvp.value);

            foreach (KVP<string, KeyCode[]> kbx in this.keyBinds)
                keyMap.Add(kbx.key, kbx.value);

            _initialized = true;
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

            if (!valueMap.TryGetValue(key, out string value))
                value = string.Empty;

            return value;
        }

        public KeyCode[] GetKey(string key)
        {
            if (!_initialized)
                Initialize();

            if (!keyMap.TryGetValue(key, out KeyCode[] combo))
                combo = new KeyCode[0];

            return combo;
        }
    }
}
