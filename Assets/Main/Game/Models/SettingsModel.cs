using Serialization;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace MPCore
{
    public class SettingsModel : Models
    {
        [Header("Sound")]
        public AudioMixer mixer;
        public string[] mixerFloatParameters;

        public readonly UnityEvent Save = new UnityEvent();
        public readonly UnityEvent Load = new UnityEvent();
        public readonly UnityEvent<SettingsData> OnSerialize = new UnityEvent<SettingsData>();
        public readonly UnityEvent<SettingsData> OnDeserialize = new UnityEvent<SettingsData>();

        [NonSerialized] public SettingsData data;

        protected override void Init()
        {
            data = XMLSerialization.Load<SettingsData>("settings");
        }
    }
}
