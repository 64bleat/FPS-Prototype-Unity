using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
	public class CharacterSoundLibrary : ScriptableObject
	{
		[SerializeField] AudioClip[] _footstepSounds;
		[SerializeField] ImpactClip[] _impactSounds;
		[SerializeField] PainClip[] _painSounds;
		[SerializeField] AudioClip _jumpSound;

		[Serializable]
		public struct ImpactClip
		{
			public AudioClip audioClip;
			public float speedThreshold;
		}

		[Serializable]
		public struct PainClip
		{
			public AudioClip audioClip;
			public int damageThreshold;
		}

		public PainClip[] PainClips => _painSounds;
		public ImpactClip[] ImpactSounds => _impactSounds;
		public AudioClip[] FootStepSounds => _footstepSounds;
		public AudioClip JumpSound => _jumpSound;
	}
}
