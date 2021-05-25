using MPCore;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    public class GUIWindowSpawner : MonoBehaviour
    {
        [SerializeField] private ObjectEvent promptWindowChannel;

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

                if (go.GetComponent<Window>() is var window && window)
                {
                    if (window.Contents.GetComponentInChildren<TextMeshProUGUI>() is var ptext && ptext)
                        ptext.text = spawn.message;

                    window.Title = spawn.title;
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
