using UnityEngine;

namespace MPCore
{
    public class BroadcastEvent : MonoBehaviour
    {
        public ObjectEvent channel;

        public void BroadcastGameObject(GameObject v)
        {
            channel.Invoke(v);
        }

        public void BroadcastString(string v)
        {
            channel.Invoke(v);
        }

        public void BroadcastInt(int v)
        {
            channel.Invoke(v);
        }

        public void BroadcastFloat(float v)
        {
            channel.Invoke(v);
        }
    }
}
