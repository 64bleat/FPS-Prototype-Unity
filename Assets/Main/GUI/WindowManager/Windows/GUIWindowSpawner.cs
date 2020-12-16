using MPGUI;
using UnityEngine;
using TMPro;

public class GUIWindowSpawner : MonoBehaviour
{
    public ObjectBroadcaster promptWindowChannel;

    private void Awake()
    {
        if (promptWindowChannel)
            promptWindowChannel.Subscribe(SpawnWindow);
    }

    private void OnDestroy()
    {
        if (promptWindowChannel)
            promptWindowChannel.Unsubscribe(SpawnWindow);
    }

    private void SpawnWindow(object obj)
    {
        if(obj is SpawnWindowEvent spawn && spawn
            && spawn.windowTemplate)
        {
            GameObject go = Instantiate(spawn.windowTemplate, transform);

            if(go.GetComponent<GUIWindow>() is var window && window )
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
