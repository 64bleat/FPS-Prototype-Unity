using UnityEngine;
using TMPro;

namespace MPCore
{
    public class MessageFader : MonoBehaviour
    {
        public float lifeTime = 3;
        public AnimationCurve fade;
        public MessageEvent messager;

        private float life;
        private TextMeshProUGUI text;

        private void Awake()
        {
            TryGetComponent(out text);
            messager.Add(Message);
        }

        private void OnDestroy()
        {
            messager.Remove(Message);
        }

        private void Update()
        {
            life -= Time.deltaTime;

            if (life > 0)
            {
                Color color = text.color;
                color.a = fade.Evaluate((lifeTime - life) / lifeTime);
                text.color = color;
            }
            else
                gameObject.SetActive(false);
        }

        private void Message(MessageEventParameters message)
        {
            text.SetText(message.message);
            life = lifeTime;
            gameObject.SetActive(true);
        }
    }
}
