using UnityEngine;
using TMPro;

public class StringBroadcastMonoEvents : MonoBehaviour
{
    public StringBroadcaster onBroadcast;
    public bool initializeOnAwake = true;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        onBroadcast.Subscribe(SetText, initializeOnAwake);
    }

    private void OnDisable()
    {
        onBroadcast.Unsubscribe(SetText);
    }

    private void SetText(string text)
    {
        this.text.text = text;
    }
}
