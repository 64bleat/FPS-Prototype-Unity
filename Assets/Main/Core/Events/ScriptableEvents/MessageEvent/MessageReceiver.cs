using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    public class MessageReceiver : MonoBehaviour
    {
        [SerializeField] private MessageEvent onMessageRecieve;
        [SerializeField] private GameObject template;
        [SerializeField] private float lifeSpan = 3f;

        private void OnEnable()
        {
            onMessageRecieve.Add(SpawnMessage);
        }

        private void OnDisable()
        {
            onMessageRecieve.Remove(SpawnMessage);
        }

        //public void Message(string message)
        //{
        //    SpawnMessage(new MessageEventParameters() { message = message });
        //}

        private void SpawnMessage(MessageEventParameters message)
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
