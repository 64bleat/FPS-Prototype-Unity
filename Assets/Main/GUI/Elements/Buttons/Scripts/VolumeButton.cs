using UnityEngine;
using UnityEngine.Audio;

namespace MPGUI
{
    /// <summary>
    ///     Sets the volume scale for a channel from 0% to 100%
    /// </summary>
    [RequireComponent(typeof(FloatButton))]
    public class VolumeButton : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string mixerFloatParameter = "Master";

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

        public void SetLevel(float volumePercent)
        {
            float displayPercent = Mathf.Clamp01(volumePercent);
            float clampedPercent = Mathf.Clamp(volumePercent, float.Epsilon, 1f);
            float decibels = Mathf.Log10(clampedPercent) * 20f;

            mixer.SetFloat(mixerFloatParameter, decibels);

            if (TryGetComponent(out FloatButton button))
            {
                button.SetValueText(displayPercent);
                button.value = displayPercent;
            }
        }
    }
}
