using UnityEngine;

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword


namespace MPCore
{
    public class CharacterVoice : MonoBehaviour
    {
        public float hurtVolume = 0.7f;
        public float jumpVolume = 0.1f;
        public float hurtRepeat = 1f;
        public AudioClip jumpSound;
        public HurtClip[] hurtSounds;

        //private Character character;
        //private CharacterBody body;
        private DamageEvent damageEvent;
        private AudioSource voice;
        private int damageAccumulation;
        private float hurtTimer;
        private bool hitSinceLastFrame;

        [System.Serializable]
        public struct HurtClip
        {
            public AudioClip clip;
            public int damageThreshold;
        }

        private void Awake()
        {
            TryGetComponent(out voice);

            gameObject.TryGetComponentInParent(out damageEvent);
            //transform.TryGetComponentInParent(out body);
            //transform.TryGetComponentInParent(out character);

            damageEvent.OnHit += OnHurt;
        }

        //private void OnEnable()
        //{
        //    //body.JumpCallback += PlayJump;
        //    //body.WalljumpCallback += PlayJump;
        //}

        //private void OnDisable()
        //{
        //    //body.JumpCallback -= PlayJump;
        //    //body.WalljumpCallback -= PlayJump;
        //}

        private void OnDestroy()
        {
            damageEvent.OnHit -= OnHurt;
        }

        private void Update()
        {

            if(hurtTimer > 0)
                hurtTimer -= Time.deltaTime;

            if (hurtTimer <= 0 && damageAccumulation > 0 && hitSinceLastFrame)
                PlayHurt();

            hitSinceLastFrame = false;
        }

        private void PlayHurt()
        {

            HurtClip last = default;

            foreach (HurtClip hc in hurtSounds)
                if (damageAccumulation <= hc.damageThreshold)
                    break;
                else
                    last = hc;

            if(last.clip)
            {
                voice.volume = hurtVolume;
                voice.clip = last.clip;
                voice.Play();
            }

            hurtTimer = hurtRepeat;
            damageAccumulation = 0;
        }

        public void PlayJump()
        {
            voice.volume = jumpVolume;
            voice.clip = jumpSound;
            voice.Play();
        }

        private void OnHurt(DamageTicket ticket)
        {
            damageAccumulation += ticket.damage;
            hitSinceLastFrame = true;
        }
    }
}
