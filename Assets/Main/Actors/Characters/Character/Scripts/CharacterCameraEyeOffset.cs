using UnityEngine;

namespace MPCore
{
    public class CharacterCameraEyeOffset : MonoBehaviour
    {
        public float eyeOffset = 0.15f;

        private CapsuleCollider cap;

        private void Awake()
        {
            cap = GetComponentInParent<CapsuleCollider>();
        }

        void FixedUpdate()
        {
            // TODO: Add OnHeightChanged(float height) to CharacterBody 
            transform.localPosition = Vector3.up * (cap.height / 2f - eyeOffset);
        }
    }
}
