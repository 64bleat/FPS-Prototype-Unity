using UnityEngine;
using UnityEngine.Serialization;

namespace MPWorld
{
	/// <summary> A script to open and close a hinged door upon interacting with it. </summary>
	[RequireComponent(typeof(AudioSource))]
	public class Door_Basic : MonoBehaviour, IInteractable
	{
		const float DEBOUNCE_TIME = 0.25f;

		public bool clockwise = true;
		public float motorForce = 80f;
		public float motorVelocity = 2.0f;
		public float snapClosedAngle = 1.0f;
		public AudioClip openSound;
		public AudioClip closeSound;

		Quaternion _closeRotation;
		Vector3 _closePosition;
		Rigidbody _rigidbody;
		AudioSource _audioSource;
		HingeJoint _hinge;
		int _interactDirection;
		float _lastDebounce = -1;

		void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_audioSource = GetComponent<AudioSource>();
			_hinge = GetComponent<HingeJoint>();

			_closeRotation = transform.rotation;
			_closePosition = transform.position;
			_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			_interactDirection = clockwise ? -1 : 1;
			enabled = false;
		}

		void FixedUpdate()
		{
			float deltaDebounce = Time.time - _lastDebounce;
			float currentAngle = Quaternion.Angle(_closeRotation, _rigidbody.rotation);

			if (deltaDebounce > DEBOUNCE_TIME && currentAngle < snapClosedAngle)
			{
				_rigidbody.rotation = _closeRotation;
				_rigidbody.position = _closePosition;
				_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				_interactDirection = clockwise ? -1 : 1;

				_audioSource.PlayOneShot(closeSound);

				enabled = false;
			}
		}

		public void OnInteractStart(GameObject other, RaycastHit hit)
		{
			_interactDirection *= -1;

			JointMotor motor = _hinge.motor;
			motor.force = motorForce;
			motor.targetVelocity = motorVelocity * _interactDirection;
			motor.freeSpin = false;
			_hinge.motor = motor;
			_hinge.useMotor = true;

			if (!enabled)
				_audioSource.PlayOneShot(openSound);

			enabled = true;
			_lastDebounce = Time.time;
			_rigidbody.constraints = RigidbodyConstraints.None;
		}

		public void OnInteractEnd(GameObject other, RaycastHit hit)
		{
			_hinge.useMotor = false;
		}

		public void OnInteractHold(GameObject other, RaycastHit hit)
		{

		}
	}
}
