using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MPCore
{
	public class CharacterSound : MonoBehaviour
	{
		[SerializeField] AudioSource footstepSource;
		[SerializeField] AudioSource impactSource;
		[SerializeField] AudioSource pickupSource;
		[SerializeField] float stepVolume = 0.2f;
		[SerializeField] float stepWalkRate = 0.61f;
		[SerializeField] float stepSprintRate = 0.3f;
		[SerializeField] AudioClip[] defaultFootstepSounds;
		[SerializeField] ImpactClip[] defaultImpactSounds;

		CharacterBody _body;
		float _footStepTimer;

		[Serializable]
		public struct ImpactClip
		{
			public AudioClip audioClip;
			public float speedThreshold;
		}

		void Awake()
		{
			TryGetComponent(out _body);

			_body.GroundMoveCallback.AddListener(PlayFootstep);
		}

		void Update()
		{
			if (_footStepTimer > 0f)
			{
				Vector3 velocity = Vector3.ProjectOnPlane(_body.Velocity - _body.CollisionInfo.PlatformVelocity, _body.CollisionInfo.Normal);
				float clamp = Mathf.InverseLerp(_body.defaultWalkSpeed, _body.defaultSprintSpeed, velocity.magnitude);

				_footStepTimer -= Time.deltaTime / Mathf.Lerp(stepWalkRate, stepSprintRate, clamp);
				footstepSource.volume = stepVolume * Mathf.Clamp01(clamp);
			}
		}

		void PlayFootstep()
		{
			if (_footStepTimer <= 0)
			{
				AudioClip stepClip = defaultFootstepSounds[Random.Range(0, defaultFootstepSounds.Length)];

				footstepSource.pitch = Random.Range(1f, 1.2f);
				footstepSource.clip = stepClip;
				footstepSource.Play();

				_footStepTimer = 1f;
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

		public void PlayPickupSound(AudioClip pickupSound)
		{
			pickupSource.PlayOneShot(pickupSound);
		}
	}
}
