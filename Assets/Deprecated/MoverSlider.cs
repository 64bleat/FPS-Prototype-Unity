using UnityEngine;

namespace Junk
{
    /// <summary> Simple script for sliding an object back and forth. </summary>
    public class MoverSlider : MonoBehaviour
    {
        public Vector3 offset;
        public float forwardTime = 1;
        public float reverseTime = 1;
        public bool slideOn = false;

        private Vector3 origin;
        private float slideFactor;

        private void Start()
        {
            slideFactor = slideOn ? 1 : 0;
            origin = transform.position;
        }

        private void FixedUpdate()
        {
            Rigidbody r = GetComponent<Rigidbody>();

            if (r && !r.isKinematic)
            {
                Vector3 currentOffset = r.position - origin;

                if (!slideOn)
                {
                    if (Vector3.Dot(currentOffset, offset) >= 0)
                        r.velocity = -offset.normalized * offset.magnitude / reverseTime;
                    else
                        r.velocity = Vector3.zero;
                }
                else
                {
                    if (currentOffset.magnitude < offset.magnitude)
                        r.velocity = offset / forwardTime;
                    else
                        r.velocity = Vector3.zero;
                }
            }
            else
            {
                if (!slideOn)
                {
                    if (reverseTime > 0)
                        slideFactor = Mathf.Max(0, slideFactor - Time.fixedDeltaTime / reverseTime);
                    else
                        slideFactor = 0;
                }
                else
                {
                    if (forwardTime > 0)
                        slideFactor = Mathf.Min(1, slideFactor + Time.fixedDeltaTime / forwardTime);
                    else
                        slideFactor = 1;
                }

                transform.position = Vector3.Lerp(origin, origin + offset, slideFactor);
            }
        }

        public void Slide(bool slide)
        {
            slideOn = slide;
        }
    }
}