using UnityEngine;

namespace MPCore
{
    [DisallowMultipleComponent]
    public sealed class PickupRotator : MonoBehaviour
    {
        [SerializeField] private float angularSpeed = 90;

        void Update()
        {
            transform.rotation *= Quaternion.AngleAxis(angularSpeed * Time.deltaTime, Vector3.up); ;
        }
    }
}
