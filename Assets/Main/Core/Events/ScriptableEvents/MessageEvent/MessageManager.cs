using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    public class MessageManager : MonoBehaviour
    {
        public MessageEvent onMessageRecieve;
        public GameObject template;
        public bool initializeOnAwake = true;
        public float lifeSpan = 3f;

        private void OnEnable()
        {
            onMessageRecieve.Add(SetText);
        }

        private void OnDisable()
        {
            onMessageRecieve.Remove(SetText);
        }

        public void Message(string message)
        {
            SetText(new MessageEventParameters() { message = message });
        }

        private void SetText(MessageEventParameters message)
        {
            GameObject go = Instantiate(template, transform);

            if (go.TryGetComponentInChildren(out Image image))
                image.color = message.bgColor;

            if(go.TryGetComponentInChildren(out TextMeshProUGUI text))
            {
                text.SetText(message.message);
                text.color = message.color;
            }

            go.SetActive(true);

            if (lifeSpan > 0)
                Destroy(go, lifeSpan);
        }
    }
}
