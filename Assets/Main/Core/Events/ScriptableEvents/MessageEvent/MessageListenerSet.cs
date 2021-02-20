using TMPro;
using UnityEngine;

namespace MPCore
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MessageListenerSet : MonoBehaviour
    {
        public MessageEvent messageEvent;
        public bool setColor = false;

        private TextMeshProUGUI text;

        private void OnEnable()
        {
            TryGetComponent(out text);
            messageEvent.Add(Invoke);
        }

        private void OnDisable()
        {
            messageEvent.Remove(Invoke);
        }

        private void Invoke(MessageEventParameters message)
        {
            text.SetText(message.message);

            if(setColor)
                text.color = message.color;
        }
    }
}
