using UnityEngine;

namespace MPCore
{
    public class Mutable : MonoBehaviour
    {
        private void Awake()
        {
            Mutation.MutateGameObject(gameObject);
        }
    }
}
