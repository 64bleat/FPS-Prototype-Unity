using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.MPWorld
{
    public class RandomSoundGenerator : MonoBehaviour
    {
        public List<AudioClip> sounds;
        public float initialWaitTime = 3f;
        public float randomIntervalMax = 5f;
        public float randomIntervalMin = 1f;

        [Header("AudioSource properties")]
        [Range(0, 1)]
        public float volume = 1f;
        [Range(0, 1)]
        public float spatialBlend = 1f;
        public float minDistance = 10f, maxDistance = 1000f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
        public bool spatialize = true;
        public bool repeat = true;

        private AudioSource audioSource;

        public void Awake()
        {
            if (sounds.Count <= 0)
                gameObject.SetActive(false);

            if ((audioSource = GetComponent<AudioSource>()) == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.spatialize = spatialize;
            audioSource.rolloffMode = rolloffMode;
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = volume;
        }

        public void Start()
        {
            Invoke("PlaySound", Random.Range(Mathf.Max(initialWaitTime, randomIntervalMin), randomIntervalMax));
        }

        void PlaySound()
        {
            if (gameObject.activeSelf)
            {
                audioSource.clip = sounds[Random.Range(0, sounds.Count)];

                if (audioSource.clip != null)
                {
                    audioSource.Play();
                    Invoke("PlaySound", Random.Range(Mathf.Max(audioSource.clip.length, randomIntervalMin), Mathf.Max(randomIntervalMin, randomIntervalMax, audioSource.clip.length)));
                }
                else
                    Invoke("PlaySound", Random.Range(randomIntervalMin, randomIntervalMax));

                if (!repeat)
                    Destroy(this);
            }
        }
    }
}