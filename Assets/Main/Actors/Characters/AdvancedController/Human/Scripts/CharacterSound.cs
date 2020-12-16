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

        public AudioClip[] defaultFootstepSounds;
        public ImpactClip[] defaultImpactSounds;

        [System.Serializable]
        public class ImpactClip
        {
            public AudioClip audioClip;
            public float speedThreshold = 0f;
        }

        public void PlayFootstep()
        {
            if (footstepSource)
            {
                AudioClip playClip = defaultFootstepSounds[Random.Range(0, defaultFootstepSounds.Length)];

                if (playClip)
                {
                    footstepSource.pitch = Random.Range(1f, 1.2f);
                    footstepSource.PlayOneShot(playClip);
                }
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
