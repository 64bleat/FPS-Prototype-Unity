using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Serialization;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.Xml.Schema;
using MPGUI;

namespace MPCore
{
    [System.Serializable]
    [XMLSurrogate(typeof(SettingsSerializationData))]
    public class SettingsSerializationData : XMLSurrogate
    {
        [XmlArray("Settings")]
        public List<KVP> values = new List<KVP>();

        [XmlArray("Keys")]
        [XmlArrayItem(typeof(KeyBindXml), ElementName = "Bind")]
        public List<KeyBindXml> keyBinds = new List<KeyBindXml>();

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

        public override XMLSurrogate Deserialize(dynamic o)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            Dictionary<string, KeyCode[]> keys = new Dictionary<string, KeyCode[]>();

            foreach (var kvp in this.values)
                values.Add(kvp.key, kvp.value);

            foreach (KeyBindXml kbx in this.keyBinds)
                keys.Add(kbx.name, kbx.keys);

            if(o is CanvasScaler cs)
            {
                if (values.TryGetValue("scaleFactor", out string intstr))
                    if (int.TryParse(intstr, out int intval))
                        cs.scaleFactor = intval;
            }
            else if(o is KeyMapSettingsGUI kms)
            {
                foreach (ScriptFloat sf in kms.values)
                    if (values.TryGetValue(sf.name, out string floatstr))
                        if (float.TryParse(floatstr, out float floatval))
                            sf.value = floatval;

                foreach (KeyBind kb in kms.kbl.keyBinds)
                    if (keys.TryGetValue(kb.name, out KeyCode[] keyCodes))
                        kb.keyCombo = keyCodes;
            }

            return this;
        }
    }
}
