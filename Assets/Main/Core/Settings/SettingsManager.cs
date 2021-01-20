using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPConsole;
using Serialization;
using UnityEngine.UI;
using MPGUI;
using UnityEngine.Audio;

namespace MPCore
{
    //[ContainsConsoleCommands]
    public class SettingsManager : MonoBehaviour
    {
        public AudioMixer mixer;
        public string[] mixerFloatParameters;

        public void SaveSettings()
        {
            XMLSerialization.Save(SerializeSettings(), "settings");
        }

        public void LoadSettings()
        {
            SettingsSerializationData data = XMLSerialization.Load<SettingsSerializationData>("settings");

            if (data != default)
                DeserializeSettings(data);
        }

        private SettingsSerializationData SerializeSettings()
        {
            SettingsSerializationData data = new SettingsSerializationData();
            CanvasScaler cs = gameObject.GetComponentInChildren<CanvasScaler>(true);
            KeyMapSettingsGUI kms = gameObject.GetComponentInChildren<KeyMapSettingsGUI>(true);

            if (cs)
                data.Serialize(cs);
            if (kms)
                data.Serialize(kms);
            if (mixer)
                data.Serialize(mixer, mixerFloatParameters);

            return data;
        }

        private void DeserializeSettings(SettingsSerializationData data)
        {
            CanvasScaler cs = gameObject.GetComponentInChildren<CanvasScaler>(true);
            KeyMapSettingsGUI kms = gameObject.GetComponentInChildren<KeyMapSettingsGUI>(true);

            data.InitializeDeserializationDictionary();

            if (cs)
                data.Deserialize(cs);
            if (kms)
                data.Deserialize(kms);
            if (mixer)
                data.Deserialize(mixer, mixerFloatParameters);
        }
    }
}
