using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class CharacterSound : MonoBehaviour
    {
        public AudioSource footstepSource;
        public AudioSource impactSource;
        public AudioSource pickupSource;
        public float stepVolume = 0.2f;
        public float stepWalkRate = 0.61f;
        public float stepSprintRate = 0.3f;

        public AudioClip[] defaultFootstepSounds;
        public ImpactClip[] defaultImpactSounds;

        private CharacterBody body;
        private float footStepTimer;

        [System.Serializable]
        public struct ImpactClip
        {
            public AudioClip audioClip;
            public float speedThreshold;
        }

        private void Awake()
        {
            TryGetComponent(out body);

            body.GroundMoveCallback += PlayFootstep;
        }

        private void Update()
        {
            if (footStepTimer > 0f)
            {
                Vector3 velocity = Vector3.ProjectOnPlane(body.Velocity - body.cb.PlatformVelocity, body.cb.Normal);
                float clamp = Mathf.InverseLerp(body.defaultWalkSpeed, body.defaultSprintSpeed, velocity.magnitude);

                footStepTimer -= Time.deltaTime / Mathf.Lerp(stepWalkRate, stepSprintRate, clamp);
                footstepSource.volume = stepVolume * Mathf.Clamp01(clamp);
            }
        }

        public void PlayFootstep()
        {
            if (footStepTimer <= 0)
            {
                AudioClip stepClip = defaultFootstepSounds[Random.Range(0, defaultFootstepSounds.Length)];

                footstepSource.pitch = Random.Range(1f, 1.2f);
                footstepSource.clip = stepClip;
                footstepSource.Play();

                footStepTimer = 1f;
            }
        }

        public void PlayImpact(float impactSpeed)
        {
            if (impactSource)
            {
                AudioClip playClip = null;

                foreach (ImpactClip clip in defaultImpactSounds)
                    if (clip.speedThreshold <= impactSpeed)
                        playClip = clip.audioClip;
                    else
                        break;

                if (playClip && impactSource && impactSource.enabled)
                    impactSource.PlayOneShot(playClip);
            }
        }
    }
}
