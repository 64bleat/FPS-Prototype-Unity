using UnityEngine;

namespace Junk
{
    public class Offsetter : MonoBehaviour
    {
        public Vector3 offset;

        private void OnTriggerEnter(Collider other)
        {
            other.transform.position += offset;
        }
    }
}
