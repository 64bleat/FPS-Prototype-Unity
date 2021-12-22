using UnityEngine;
using System.Linq;

namespace MPCore
{
	public class CharacterVoice : MonoBehaviour
	{
		[SerializeField] float hurtVolume = 0.7f;
		[SerializeField] float jumpVolume = 0.1f;
		[SerializeField] float hurtRepeat = 1f;
		[SerializeField] float recoveryRate = 25f;
		[SerializeField] AudioClip jumpSound;
		[SerializeField] HurtClip[] hurtSounds;

		DamageEvent _damage;
		AudioSource _voiceSource;
		float _damageAccumulation;
		float _hurtSoundCoolDown;
		bool _hitSinceLastFrame;
		float _hurtSoundThreshold;
		[System.Serializable]
		public struct HurtClip
		{
			public AudioClip clip;
			public int damageThreshold;
		}

		private void Awake()
		{
			_hurtSoundThreshold = hurtSounds.Select(s => s.damageThreshold).Min();

			TryGetComponent(out _voiceSource);
			gameObject.TryGetComponentInParent(out _damage);

			_damage.OnHit += OnHurt;
		}

		private void OnDestroy()
		{
			_damage.OnHit -= OnHurt;
		}

		private void Update()
		{

			if(_hurtSoundCoolDown > 0)
				_hurtSoundCoolDown -= Time.deltaTime;

			if (_hurtSoundCoolDown <= 0 && _damageAccumulation > _hurtSoundThreshold && _hitSinceLastFrame)
				PlayHurt();

			_damageAccumulation = Mathf.MoveTowards(_damageAccumulation, 0f, recoveryRate * Time.deltaTime);
			_hitSinceLastFrame = false;
		}

		private void PlayHurt()
		{

			HurtClip last = default;

			foreach (HurtClip hc in hurtSounds)
				if (_damageAccumulation <= hc.damageThreshold)
					break;
				else
					last = hc;

			if(last.clip)
			{
				_voiceSource.volume = hurtVolume;
				_voiceSource.clip = last.clip;
				_voiceSource.Play();
			}

			_hurtSoundCoolDown = hurtRepeat;
			_damageAccumulation = 0;
		}

		public void PlayJump()
		{
			_voiceSource.volume = jumpVolume;
			_voiceSource.clip = jumpSound;
			_voiceSource.Play();
		}

		private void OnHurt(DamageTicket ticket)
		{
			_damageAccumulation += ticket.deltaValue;
			_hitSinceLastFrame = true;
		}
	}
}
