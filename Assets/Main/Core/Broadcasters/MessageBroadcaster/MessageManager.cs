using TMPro;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public MessageBroadcaster onMessageRecieve;
    public GameObject template;
    public bool initializeOnAwake = true;
    public float lifeSpan = 3f;

    private void OnEnable()
    {
        onMessageRecieve.Subscribe(SetText);
    }

    private void OnDisable()
    {
        onMessageRecieve.Unsubscribe(SetText);
    }

    private void SetText(MessageInfo message)
    {
        GameObject c = Instantiate(template, transform);
        TextMeshProUGUI t = c.GetComponentInChildren<TextMeshProUGUI>();

        t.text = message.message;
        c.SetActive(true);

        if (lifeSpan > 0)
            Destroy(c, lifeSpan);
    }
}
