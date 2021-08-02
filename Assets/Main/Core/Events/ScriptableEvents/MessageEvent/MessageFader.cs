using UnityEngine;
using TMPro;
using System.Collections;

namespace MPCore
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MessageFader : MonoBehaviour
    {
        [SerializeField] private float _duration = 3;
        [SerializeField] private AnimationCurve fade;
        public MessageEvent messager;

        private TextMeshProUGUI _text;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            messager.Add(Message);
        }

        private void OnEnable()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(StartFadeCoroutine());
        }

        private void OnDestroy()
        {
            messager.Remove(Message);
        }

        private IEnumerator StartFadeCoroutine()
        {
            float startTime = Time.time;

            while(true)
            {
                float duration = Time.time - startTime;

                if (duration < _duration)
                {
                    Color color = _text.color;
                    color.a = fade.Evaluate(duration / _duration);
                    _text.color = color;
                }
                else
                    break;

                yield return null;
            }

            gameObject.SetActive(false);
            _fadeCoroutine = null;
        }

        private void Message(MessageEventParameters message)
        {
            gameObject.SetActive(false);
            _text.SetText(message.message);
            gameObject.SetActive(true);
        }
    }
}
