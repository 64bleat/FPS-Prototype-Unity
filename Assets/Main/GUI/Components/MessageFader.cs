using UnityEngine;
using TMPro;
using System.Collections;

namespace MPCore
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MessageFader : MonoBehaviour
    {
        [SerializeField] float _duration = 3;
        [SerializeField] AnimationCurve fade;
        //public MessageEvent messager;

        TextMeshProUGUI _text;
        Coroutine _fadeCoroutine;

        void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            //messager.Add(Message);
        }

        void OnEnable()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(Fade());
        }

        void OnDestroy()
        {
            //messager.Remove(Message);
        }

        IEnumerator Fade()
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

        void Message(MessageEventParameters message)
        {
            gameObject.SetActive(false);
            _text.SetText(message.message);
            gameObject.SetActive(true);
        }
    }
}
