using MPCore;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    public class GUIWindowSpawner : MonoBehaviour
    {
        public ObjectEvent promptWindowChannel;

        private void Awake()
        {
            if (promptWindowChannel)
                promptWindowChannel.Add(SpawnWindow);
        }

        private void OnDestroy()
        {
            if (promptWindowChannel)
                promptWindowChannel.Remove(SpawnWindow);
        }

        private void SpawnWindow(object obj)
        {
            if (obj is SpawnWindowHook spawn && spawn
                && spawn.windowTemplate)
            {
                GameObject go = Instantiate(spawn.windowTemplate, transform);

                if (go.GetComponent<GUIWindow>() is var window && window)
                {
                    if (window.panel && window.panel.GetComponentInChildren<TextMeshProUGUI>() is var ptext && ptext)
                        ptext.text = spawn.message;

                    if (window.title && window.title.GetComponentInChildren<TextMeshProUGUI>() is var ttext && ttext)
                        ttext.text = spawn.title;
                }

                if (go.transform is RectTransform rect && rect)
                {
                    rect.offsetMax = new Vector2(rect.offsetMax.x, spawn.windowHeight / 2);
                    rect.offsetMin = new Vector2(rect.offsetMin.x, -spawn.windowHeight / 2);
                }

                go.SetActive(true);
            }
        }
    }
}
