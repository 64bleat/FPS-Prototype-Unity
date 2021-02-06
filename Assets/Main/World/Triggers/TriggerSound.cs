using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    public class TriggerSound : MonoBehaviour
    {
        [Tooltip("Play one of these sounds at random.")]
        public AudioClip[] sounds;

        private AudioSource audioSource = null;

        public void Awake()
        {
            if ((audioSource = GetComponent<AudioSource>()) == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void OnTrigger()
        {
            if(audioSource != null && sounds != null && sounds.Length != 0)
            {
                foreach(AudioClip c in sounds)
                    if(c != null)
                    {
                        audioSource.clip = c;
                        audioSource.Play();
                        return;
                    }
            }
        }
    }
}
