using UnityEngine;
using TMPro;

namespace MPCore
{
    public class StringEventSetText : MonoBehaviour
    {
        public StringEvent onBroadcast;
        public bool initializeOnAwake = true;

        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            onBroadcast.Add(SetText, initializeOnAwake);
        }

        private void OnDisable()
        {
            onBroadcast.Remove(SetText);
        }

        private void SetText(string text)
        {
            this.text.text = text;
        }
    }
}
