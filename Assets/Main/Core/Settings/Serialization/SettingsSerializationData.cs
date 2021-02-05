using MPGUI;
using Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace MPCore
{
    [Serializable]
    [XMLSurrogate(typeof(SettingsSerializationData))]
    public class SettingsSerializationData : XMLSurrogate
    {
        [XmlArray("Settings")]
        public List<KVP> values = new List<KVP>();

        [XmlArray("Keys")]
        [XmlArrayItem(typeof(KeyBindXml), ElementName = "Bind")]
        public List<KeyBindXml> keyBinds = new List<KeyBindXml>();

        [NonSerialized] private Dictionary<string, string> valueMap;
        [NonSerialized] private Dictionary<string, KeyCode[]> keys;

        public struct KVP
        {
            [XmlAttribute("k")] public string key;
            [XmlAttribute("v")] public string value;

            public KVP(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public struct KeyBindXml
        {
            [XmlAttribute] public string name;
            [XmlAttribute] public KeyCode[] keys;

            public KeyBindXml(string name, KeyCode[] keys)
            {
                this.name = name;
                this.keys = keys;
            }
        }

        public override XMLSurrogate Serialize(dynamic o)
        {
            if (o is CanvasScaler cs)
                values.Add(new KVP("scaleFactor",cs.scaleFactor.ToString()));
            else if(o is KeyMapSettingsGUI kms)
            {
                foreach (ScriptFloat sf in kms.values)
                    values.Add(new KVP(sf.name, sf.value.ToString()));

                foreach (KeyBind kb in kms.kbl.keyBinds)
                    keyBinds.Add(new KeyBindXml(kb.name, kb.keyCombo));
            }

            return this;
        }

        public void InitializeDeserializationDictionary()
        {
            valueMap = new Dictionary<string, string>();
            keys = new Dictionary<string, KeyCode[]>();

            foreach (var kvp in this.values)
                valueMap.Add(kvp.key, kvp.value);

            foreach (KeyBindXml kbx in this.keyBinds)
                keys.Add(kbx.name, kbx.keys);
        }

        public override XMLSurrogate Deserialize(dynamic o)
        {
            if(o is CanvasScaler cs)
            {
                if (valueMap.TryGetValue("scaleFactor", out string intstr))
                    if (int.TryParse(intstr, out int intval))
                        cs.scaleFactor = intval;
            }
            else if(o is KeyMapSettingsGUI kms)
            {
                foreach (ScriptFloat sf in kms.values)
                    if (valueMap.TryGetValue(sf.name, out string floatstr))
                        if (float.TryParse(floatstr, out float floatval))
                            sf.value = floatval;

                foreach (KeyBind kb in kms.kbl.keyBinds)
                    if (keys.TryGetValue(kb.name, out KeyCode[] keyCodes))
                        kb.keyCombo = keyCodes;
            }

            return this;
        }

        public void SerializeMixer(AudioMixer mixer, string[] floatParameters)
        {
            foreach (string paramName in floatParameters)
                if (mixer.GetFloat(paramName, out float value))
                    values.Add(new KVP(paramName, value.ToString()));
        }

        public void DeserializeMixer(AudioMixer mixer, string[] floatParameters)
        {
            foreach (string paramName in floatParameters)
                if (valueMap.TryGetValue(paramName, out string valueText)
                    && float.TryParse(valueText, out float value))
                    mixer.SetFloat(paramName, value);
        }
    }
}
