using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Used to smooth player view on stairs
    /// </summary>
    public class CharacterCamera : MonoBehaviour
    {
        [HideInInspector] public float stepOffset = 0f;

        private CharacterBody body;

        private void Awake()
        {
            body = GetComponentInParent<CharacterBody>();
            PauseManager.Add(OnPause);
        }

        private void OnDestroy()
        {
            PauseManager.Remove(OnPause);
        }

        private void OnPause(bool pause)
        {
            enabled = !pause;
        }

        private void FixedUpdate()
        {
            stepOffset = Mathf.Lerp(stepOffset, 0f, Mathf.Min(1, 12f * Time.fixedDeltaTime));

            transform.localPosition = transform.InverseTransformDirection(body.transform.up) * stepOffset;

            //if (body.currentState == CharacterBody.MoveState.Grounded)
            //{
            //    float relativeSpeed = (body.Velocity - body.cb.PlatformVelocity).magnitude;
            //    bool isMoving = !body.moveDir.Equals(Vector3.zero);

            //    if (isMoving)
            //        headBob += Vector3.up * Mathf.Sin(Time.time * 15f) * relativeSpeed / 10f * Time.fixedDeltaTime;

            //    if (Time.time - body.lastStepTime > 0.35f && isMoving && relativeSpeed > 2.9f)
            //    {
            //        if (body.characterSound)
            //            body.characterSound.PlayFootstep();
            //        body.lastStepTime = Time.time;
            //    }
            //}
            //else
            //{
            //    float mag = headBob.magnitude;

            //    mag = Mathf.Max(0, mag - 1f * Time.fixedDeltaTime);

            //    //headBob = Vector3.Lerp(headBob, Vector3.zero, Mathf.Clamp01(12f * Time.fixedDeltaTime));
            //    headBob = Vector3.ClampMagnitude(headBob, mag);
            //}
        }
    }
}
