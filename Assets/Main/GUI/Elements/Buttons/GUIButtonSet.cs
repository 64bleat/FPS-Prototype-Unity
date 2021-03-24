using System;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

namespace MPGUI
{
    /// <summary>
    /// Easily generate a set of buttons contained in this RectTransform
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GUIButtonSet : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private GameObject titlePrefab;

        private Vector2 size = Vector2.zero;

        private void Awake()
        {
            size.x = (transform as RectTransform).sizeDelta.x;
        }

        public GameObject AddButton(string buttonText, Action clickActions)
        {
            GameObject go = AddGameObject(buttonPrefab); 
            Button button = go.GetComponentInChildren<Button>();
            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();

            button.clickEvents.AddListener(new UnityAction(clickActions));
            text.text = buttonText;

            return go;
        }

        public GameObject AddTitle(string titleText)
        {
            GameObject go = AddGameObject(titlePrefab);
            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();

            text.text = titleText;

            return go;
        }

        public GameObject AddGameObject(GameObject template)
        {
            GameObject go = Instantiate(template, transform);

            go.SetActive(true);
            AddSize(go);

            return go;
        }

        public void Clear()
        {
            while (transform.childCount != 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            size = Vector2.zero;
        }

        private void AddSize(GameObject go)
        {
            size.y += (go.transform as RectTransform).sizeDelta.y;
            (transform as RectTransform).sizeDelta = size;
        }
    }
}
