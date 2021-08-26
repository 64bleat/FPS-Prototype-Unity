using UnityEngine;
using UnityEngine.Audio;

namespace MPGUI
{
    public class VolumeField : TextField
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string parameter = "Master";

        private static readonly string format = "P0";

        protected override void InitValue()
        {
            if (mixer.GetFloat(parameter, out float decibels))
            {
                float percent = Mathf.Pow(10f, decibels / 20f);
                SetField(percent.ToString(format));
            }
        }

        protected override void ParseValue(string value)
        {
            if (int.TryParse(value, out int i))
            {
                float percent = i * 0.01f;
                float clampedPercent = Mathf.Clamp(percent, float.Epsilon, 1f);
                float decibels = Mathf.Log10(clampedPercent) * 20f;

                mixer.SetFloat(parameter, decibels);
                SetField(clampedPercent.ToString(format));
            }
            else
                SetField(_recoveryText);
        }
    }
}
