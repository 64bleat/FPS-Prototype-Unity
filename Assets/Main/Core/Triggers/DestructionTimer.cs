using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Destroyes the GameObject after a specified time
    /// </summary>
    /// <remarks>
    /// Compatible with <c>GameObjectPool</c>
    /// </remarks>
    public class DestructionTimer : MonoBehaviour
    {
        [Tooltip("Life Time of the GameObject in seconds")]
        [SerializeField] private float lifeTime = 0.5f;

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
