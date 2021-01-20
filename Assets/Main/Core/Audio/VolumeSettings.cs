using MPGUI;
using UnityEngine;
using UnityEngine.Audio;

namespace MPCore
{
    /// <summary>
    /// Sets the volume scale for a channel from 0% to 100%
    /// </summary>
    [RequireComponent(typeof(GUIFloatButton))]
    public class VolumeSettings : MonoBehaviour
    {
        public AudioMixer mixer;
        public string mixerFloatParameter = "Master";

        private void OnEnable()
        {
            if(TryGetComponent(out GUIFloatButton button)
                && mixer.GetFloat(mixerFloatParameter, out float value))
            {
                value /= 20f;
                value = Mathf.Pow(10f, value);
                button.valueName.SetText(value.ToString(button.displayFormat));
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

            if (TryGetComponent(out GUIFloatButton button))
            {
                button.valueName.SetText(volumeScale.ToString(button.displayFormat));
                button.value = volumeScale;
            }
        }
    }
}
