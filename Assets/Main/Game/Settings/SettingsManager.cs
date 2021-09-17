using MPGUI;
using Serialization;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace MPCore
{
    public class SettingsManager : MonoBehaviour
    {
        private SettingsModel _settingsModel;

        private void Awake()
        {
            _settingsModel = Models.GetModel<SettingsModel>();
            _settingsModel.Save.AddListener(Save);
            _settingsModel.Load.AddListener(Load);
        }

        private void Start()
        {
            _settingsModel.Load?.Invoke();
        }

        private void OnDestroy()
        {
            Models.RemoveModel<SettingsModel>();
        }

        public void Load()
        {
            if (_settingsModel.data != null)
                Deserialize(_settingsModel.data);
        }

        void Save()
        {
            Serialize();
            XMLSerialization.Save(_settingsModel.data, "settings");
        }

        private void Serialize()
        {
            SettingsData data = new SettingsData();

            KeyModelSettingsManager kms = gameObject.GetComponentInChildren<KeyModelSettingsManager>(true);
            if (kms)
            {
                KeyModel keyModel = Models.GetModel<KeyModel>();

                //foreach (ScriptFloat sf in keyModel.values)
                //    data.SetValue(sf.name, sf.value.ToString());

                foreach (KeyBind kb in keyModel.keys)
                    data.SetKey(kb.name, kb.keyCombo);
            }

            foreach (string name in _settingsModel.mixerFloatParameters)
                if (_settingsModel.mixer.GetFloat(name, out float value))
                    data.SetValue(name, value.ToString());

            _settingsModel.OnSerialize?.Invoke(data);
            _settingsModel.data = data;
        }

        private void Deserialize(SettingsData data)
        {
            KeyModelSettingsManager kms = gameObject.GetComponentInChildren<KeyModelSettingsManager>(true);
            if (kms)
            {
                KeyModel keyModel = Models.GetModel<KeyModel>();

                //foreach (ScriptFloat sf in keyModel.values)
                //    if (float.TryParse(data.GetValue(sf.name), out float value))
                //        sf.value = value;

                foreach (KeyBind kb in keyModel.keys)
                    if (data.GetKey(kb.name).Length > 0)
                        kb.keyCombo = data.GetKey(kb.name);
            }

            foreach (string name in _settingsModel.mixerFloatParameters)
                if (float.TryParse(data.GetValue(name), out float value))
                    _settingsModel.mixer.SetFloat(name, value);

            _settingsModel.OnDeserialize?.Invoke(data);
        }
    }
}
