using UnityEngine;

namespace MPCore
{
	public class CharacterSoundManager : MonoBehaviour
	{
		const float IMPACT_DISTANCE = 2f;

		[SerializeField] CharacterSoundLibrary _characterSoundLibrary;
		[SerializeField] AudioSource _footstepSource;
		[SerializeField] AudioSource _impactSource;
		[SerializeField] AudioSource _pickupSource;
		[SerializeField] AudioSource _voiceSource;
		[SerializeField] float _stepVolume = 0.25f;
		[SerializeField] float _stepPitch = 1f;
		[SerializeField] float _stepPitchDeviation = 0.1f;
		[SerializeField] float _painVolume = 0.7f;
		[SerializeField] float _jumpVolume = 0.1f;
		[SerializeField] float _painRate = 1f;
		[SerializeField] float _painRecoveryRate = 25f;
		[SerializeField] float _stepRateMin = 0.61f;
		[SerializeField] float _stepRateMax = 0.3f;
		[SerializeField] float _impactRate = 0.25f;

		CharacterBody _body;
		float _lastPainTime;
		float _lastImpactTime;
		float _lastFootStepTime;
		float _storedDamage;

		void Awake()
		{
			_body = GetComponent<CharacterBody>();
			_footstepSource.volume = _stepVolume;
			_body.GroundMoveCallback.AddListener(PlayFootStepSound);
		}

		void Update()
		{
			_storedDamage = Mathf.MoveTowards(_storedDamage, 0f, _painRecoveryRate * Time.deltaTime);
		}

		public void PlayFootStepSound()
		{
			Vector3 relativeVelocity = _body.Velocity - _body.CollisionInfo.PlatformVelocity;

			if (relativeVelocity.magnitude > _body.defaultWalkSpeed * 0.5f)
			{
				float currentTime = Time.time;
				float deltaTime = currentTime - _lastFootStepTime;
				float groundSpeed = Vector3.ProjectOnPlane(relativeVelocity, _body.CollisionInfo.FloorNormal).magnitude;
				float tLerp = Mathf.InverseLerp(_body.defaultWalkSpeed, _body.defaultSprintSpeed, groundSpeed);
				float stepRate = Mathf.Lerp(_stepRateMin, _stepRateMax, tLerp);

				if (deltaTime >= stepRate)
				{
					int index = Random.Range(0, _characterSoundLibrary.FootStepSounds.Length);
					_footstepSource.clip = _characterSoundLibrary.FootStepSounds[index];
					_footstepSource.pitch = _stepPitch + Random.Range(-_stepPitchDeviation, _stepPitchDeviation);
					_footstepSource.Play();
					_lastFootStepTime = currentTime;
				}
			}
		}

		public void PlayHurtSound(float damage)
		{
			float currentTime = Time.time;
			float deltaTime = currentTime - _lastPainTime;

			_storedDamage += damage;

			if (deltaTime > _painRate)
			{
				foreach (var painClip in _characterSoundLibrary.PainClips)
				{
					if (painClip.damageThreshold >= _storedDamage)
					{
						_storedDamage = 0;
						_lastPainTime = currentTime;
						_voiceSource.clip = painClip.audioClip;
						_voiceSource.volume = _painVolume;
						_voiceSource.Play();
						return;
					}
				}
			}
		}

		public void PlayJumpSound()
		{
			_voiceSource.volume = _jumpVolume;
			_voiceSource.clip = _characterSoundLibrary.JumpSound;
			_voiceSource.Play();
		}

		public void PlayImpactSound(Vector3 direction, float magnitude)
		{
			foreach(var impactClip in _characterSoundLibrary.ImpactSounds)
			{
				if(impactClip.speedThreshold <= magnitude)
				{
					_lastImpactTime = Time.time;
					_impactSource.transform.position = _body.View.position + direction * IMPACT_DISTANCE;
					_impactSource.clip = impactClip.audioClip;
					_impactSource.Play();
					return;
				}
			}
		}

		public void PlayPickupSound(AudioClip pickupClip)
		{
			_pickupSource.PlayOneShot(pickupClip);
		}
	}
}
