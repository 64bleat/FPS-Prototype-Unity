using UnityEngine;

namespace MPCore
{
    public class DestroyGameObjectMethods : MonoBehaviour
    {
        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }

        public void DestroyGameObjectImmediate()
        {
            DestroyImmediate(gameObject);
        }
    }
}
