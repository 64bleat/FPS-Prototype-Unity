using UnityEngine;

namespace MPCore
{
    public class Particle : MonoBehaviour
    {
        public float lifeTime = 1f;
        public float life = 1;

        private void OnEnable()
        {
            life = lifeTime;

        }

        void Update()
        {
            life -= Time.deltaTime;

            transform.localScale = Vector3.one * 1f * Mathf.Max(0, life / lifeTime);

            if (life <= 0)
                GameObjectPool.DestroyMember(gameObject);
        }
    }
}
