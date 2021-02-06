using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Disables gameObject after a specif time
    /// </summary>
    public class DisableTimer : MonoBehaviour
    {
        public float lifeTime = 0.5f;

        private float time;

        private void OnEnable()
        {
            time = lifeTime;
        }

        private void Update()
        {
            time -= Time.deltaTime;

            if (time <= 0)
                GameObjectPool.Return(gameObject);
        }
    }
}
