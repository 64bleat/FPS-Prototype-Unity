using UnityEngine;

namespace MPCore
{
    public class CharacterCameraEyeOffset : MonoBehaviour
    {
        public float eyeOffset = 0.15f;

        CapsuleCollider _cap;

        void Awake()
        {
            _cap = GetComponentInParent<CapsuleCollider>();
        }

        void FixedUpdate()
        {
            transform.localPosition = Vector3.up * (_cap.height / 2f - eyeOffset);
        }
    }
}
