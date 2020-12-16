using UnityEngine;

namespace Junk
{
    public class KinematicLock : MonoBehaviour
    {
        public Rigidbody[] BodiesToLock;

        private Vector3[] positionOffsets;
        private Quaternion[] rotationOffsets;

        private void Start()
        {
            positionOffsets = new Vector3[BodiesToLock.Length];
            rotationOffsets = new Quaternion[BodiesToLock.Length];

            for (int i = 0; i < BodiesToLock.Length; i++)
            {
                positionOffsets[i] = BodiesToLock[i].transform.position - transform.position;
                rotationOffsets[i] = BodiesToLock[i].transform.rotation * Quaternion.Inverse(transform.rotation);
            }
        }

        private void Update()
        {
            for (int i = 0; i < BodiesToLock.Length; i++)
            {
                BodiesToLock[i].MovePosition(positionOffsets[i] + transform.position);
                BodiesToLock[i].MoveRotation(rotationOffsets[i] * transform.rotation);
            }
        }
    }
}
