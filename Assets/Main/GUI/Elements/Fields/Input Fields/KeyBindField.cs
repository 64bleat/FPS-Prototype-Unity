using MPCore;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    public class KeyBindField : MonoBehaviour, IGUISelectable
    {
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private KeyBind key;

        private static readonly List<KeyCode> keyBuffer = new List<KeyCode>();

        private void Awake()
        {
            if(key)
                SetKey(key);

            enabled = false;
        }

        private void OnDisable()
        {
            CancelReassignment();
        }

        private void Update()
        {
            bool wait = true;

            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKey(k))
                {
                    wait = false;

                    if (Input.GetKeyDown(k) && !keyBuffer.Contains(k))
                        keyBuffer.Add(k);
                }

            if (keyBuffer.Count != 0 && wait)
                CommitReassignment();

        }

        public void SetKey(KeyBind k)
        {
            key = k;
            description.SetText(k.name);
            valueText.SetText(k.GetComboString());
        }

        private void CancelReassignment()
        {
            string val = key ? key.GetComboString() : "!KEY";

            valueText.SetText(val);
            enabled = false;
        }

        private void BeginReassignment()
        {
            keyBuffer.Clear();
            enabled = true;

            valueText.SetText("???");
        }

        private void CommitReassignment()
        {
            if (enabled && keyBuffer.Count != 0)
                key.keyCombo = keyBuffer.ToArray();

            valueText.SetText(key.GetComboString());

            if(gameObject.TryGetComponentInParent(out GUIInputManager ginput))
                ginput.Deselect(GetComponent<IGUISelectable>());

            enabled = false;
        }

        public void OnSelect()
        {
            if (gameObject.TryGetComponent(out InputManager input))
                input.enabled = false;

            BeginReassignment();
        }

        public void OnDeselect()
        {
            if (gameObject.TryGetComponent(out InputManager input))
                input.enabled = true;

            CancelReassignment();
        }
    }
}
