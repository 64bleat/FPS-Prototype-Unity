using UnityEngine;

namespace MPCore
{
    public class SpawnWindowHook : MonoBehaviour
    {
        public ObjectEvent windowChannel;
        public string title;
        [TextArea]
        public string message;
        public float windowHeight = 128;

        public GameObject windowTemplate;

        public void SpawnWindow()
        {
            if (windowChannel)
                windowChannel.Invoke(this);
        }
    }
}