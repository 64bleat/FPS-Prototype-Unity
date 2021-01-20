using UnityEngine;

namespace MPCore
{
    public class DisableTimer : MonoBehaviour
    {
        public float lifeTime = 0.5f;

        private float currentLife;

        private void OnEnable()
        {
            currentLife = lifeTime;
        }

        private void Update()
        {
            if ((currentLife -= Time.deltaTime) <= 0)
                GameObjectPool.Return(gameObject);
        }
    }
}
