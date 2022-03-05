using MPCore;
using UnityEngine;

namespace MPWorld
{
	[RequireComponent(typeof(Rigidbody))]
	public class SpaceShip : MonoBehaviour
	{
		public FloatLever rotationLever;
		public FloatLever speedLever;
		public FloatLever rollLever;
		public Vector2Lever turnLever;
		public PushButton antiGravityButton;
		public PushButton autoCorrectButton;
		public PushButton reverseButton;
		public PushButton panLever;

		public float maxSpeed = 10f;
		public float maxAcceleration = 500f;
		public float maxRollAcceleration = 0.5f;
		public float maxAngularAcceleration = 0.5f;

		public float autoLevelPower = 2f;
		public float autoLevelScale = 10f;

		Rigidbody _rigidbody;
		float _revMultiplier;

		void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		void Start()
		{
			antiGravityButton.dataValue.Subscribe(antiGrav => _rigidbody.useGravity = !antiGrav.newValue);
			reverseButton.dataValue.Subscribe(rev => _revMultiplier = rev.newValue ? -0.5f : 1f);
		}

		void FixedUpdate()
		{
			float currentSpeed = Mathf.Abs(Vector3.Dot(_rigidbody.velocity, -transform.forward));
			float acceleration = speedLever.Value * maxAcceleration;
			float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
			Vector3 sideVelocity = Vector3.ProjectOnPlane(_rigidbody.velocity, transform.forward);

			//forward acceleration
			_rigidbody.AddForce(-transform.forward * acceleration * _revMultiplier * (1f-speedFactor), ForceMode.Acceleration);

			//focus Acceleration
			if (antiGravityButton.dataValue.Value && !panLever.dataValue.Value)
				_rigidbody.AddForce(-sideVelocity * 10, ForceMode.Acceleration);

			if (!panLever.dataValue.Value)
			{
				_rigidbody.AddForceAtPosition(transform.up * maxAngularAcceleration * rotationLever.Value * turnLever.Value.y, _rigidbody.worldCenterOfMass - transform.forward * 10f, ForceMode.Acceleration);
				_rigidbody.AddForceAtPosition(-transform.right * maxAngularAcceleration * rotationLever.Value * turnLever.Value.x, _rigidbody.worldCenterOfMass - transform.forward * 10f, ForceMode.Acceleration);
			}
			else
			{
				_rigidbody.AddForce((transform.up * turnLever.Value.y - transform.right * turnLever.Value.x) * rotationLever.Value * maxAcceleration * 0.25f, ForceMode.Acceleration);
			}

			//Roll
			_rigidbody.AddForceAtPosition(transform.right * maxRollAcceleration * (rollLever.Value - 0.5f) * 2f, _rigidbody.worldCenterOfMass - transform.up * 10f, ForceMode.Acceleration);

			if (autoCorrectButton.dataValue.Value)
			{
				Vector3 leveling = Vector3.ProjectOnPlane(-Physics.gravity.normalized - _rigidbody.transform.up, _rigidbody.transform.up);
				leveling = leveling.normalized * Mathf.Pow(leveling.magnitude, autoLevelPower);


				_rigidbody.AddForceAtPosition(leveling, _rigidbody.worldCenterOfMass + transform.up * autoLevelScale, ForceMode.Acceleration);
			}
		}
	}
}
