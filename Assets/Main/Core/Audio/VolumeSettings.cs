using MPGUI;
using UnityEngine;
using UnityEngine.Audio;

namespace MPCore
{
    /// <summary>
    /// Sets the volume scale for a channel from 0% to 100%
    /// </summary>
    [RequireComponent(typeof(FloatButton))]
    public class VolumeSettings : MonoBehaviour
    {
        public AudioMixer mixer;
        public string mixerFloatParameter = "Master";

        private void OnEnable()
        {
            if(TryGetComponent(out FloatButton button)
                && mixer.GetFloat(mixerFloatParameter, out float value))
            {
                value /= 20f;
                value = Mathf.Pow(10f, value);
                button.SetValueText(value);
                button.value = value;
            }
        }

        public void SetLevel(float volumeScale)
        {
            float decibels;

            volumeScale = Mathf.Clamp01(volumeScale);
            decibels = Mathf.Max(volumeScale, float.Epsilon);
            decibels = Mathf.Log10(decibels);
            decibels *= 20;
            mixer.SetFloat(mixerFloatParameter, decibels);

            if (TryGetComponent(out FloatButton button))
            {
                button.SetValueText(volumeScale);
                button.value = volumeScale;
            }
        }
    }
}
