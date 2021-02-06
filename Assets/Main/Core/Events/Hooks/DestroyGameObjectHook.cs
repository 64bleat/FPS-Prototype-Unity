using UnityEngine;

namespace MPCore
{
    public class DestroyGameObjectHook : MonoBehaviour
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
