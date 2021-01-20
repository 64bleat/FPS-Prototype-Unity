﻿using TMPro;
using UnityEngine;

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
            GameObject c = Instantiate(template, transform);
            TextMeshProUGUI t = c.GetComponentInChildren<TextMeshProUGUI>();

            t.text = message.message;
            c.SetActive(true);

            if (lifeSpan > 0)
                Destroy(c, lifeSpan);
        }
    }
}