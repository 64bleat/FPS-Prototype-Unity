using MPGUI;
using MPWorld;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
	/// <summary>
	/// Controls character movement
	/// </summary>
	public class CharacterBody : MonoBehaviour
	{
		const float INSET = 0.05f;
		const float DOWNCAST_GROUNDED = 3f;
		const float DOWNCAST_AIRBORNE = 1.5f;

		public enum MoveState { Grounded, Airborne }
		static readonly string[] _collisionLayers = { "Default", "Physical", "Player" };
		static readonly Collider[] _colliderBuffer = new Collider[20];
		static int _layerMask;

		// SERIALIZED
		[Header("Abilities")]
		public bool enableAlwaysRun = true;
		public bool enableCrouch = true;
		public bool enableJump = true;
		[Header("Modules")]
		public WallJumpBoots wallJump;
		public WallClimbGloves wallClimb;
		[Header("Movement")]
		public float defaultWalkSpeed = 2.15f;
		public float defaultMoveSpeed = 5.5f;
		public float defaultStrideSpeed = 7f;
		public float defaultSprintSpeed = 7.5f;
		public float defaultGroundAcceleration = 40f;
		public float defaultStrideAcceleration = 1f;
		public float defaultGroundDecceleration = 25f;
		public float defaultAirAcceleration = 6f;
		public float defaultSpeedDecceleration = 2.5f;
		public float defaultslopeLimit = 46;
		public int fallForgiveness = 5;
		[Header("Crouching")]
		public float defaultCrouchDownSpeed = 5.5f;
		public float defaultCrouchUpSpeed = 4f;
		[Header("Jumping")]
		public float defaultJumpHeight = 0.8f;
		public float defaultCrouchJumpHeight = 0.8f;
		public float defaultBunnyHopRate = 0.5f;
		public float defaultMaxKickVelocity = 15f;
		[Header("Gliding")]
		public float defaultGlideAngle = 60f;
		public float defaultSpeedToLift = 9f;
		public float defaultDrag = 2.5f;
		public float defaultJetAccel = 0f;
		public float defaultJetSpeed = 200f;
		[Header("Other")]
		public DataValue<float> height = new(1.65f);
		public bool leftHanded = false;
		public float defaultHeight = 1.65f;
		public float defaultCrouchHeight = 0.7f;
		public float defaultStepOffset = 0.3f;
		public float defaultMaxSafeImpactSpeed = 11f;
		public float defaultGroundStickThreshold = 11f;
		public float defaultMass = 80f;
		[Header("Components")]
		public GameObject thirdPersonBody;
		public Transform cameraAnchor;
		public Transform cameraSlot;
		public Transform cameraHand;
		public Transform rightHand;
		public Transform leftHand;
		public GameObject deadBody;
		[Header("References")]
		[SerializeField] DamageType impactDamageType;
		[Header("Events")]
		public UnityEvent JumpCallback;
		public UnityEvent WalljumpCallback;
		public UnityEvent GroundMoveCallback;
		public UnityEvent<CharacterBody> OnGlide;
		public UnityEvent<CharacterBody> OnWallJump;

		[NonSerialized] public Vector3 falseGravity;
		[NonSerialized] public float lookY = 0;
		[NonSerialized] public MoveState currentState;

		GUIModel _guiModel;
		GameModel _gameModel;
		CharacterSoundManager _sound;
		Character _character;
		CharacterView _cameraRig;
		GravitySampler _physics;
		DamageEvent _damageEvent;
		CollisionBuffer _cb;
		CapsuleCollider _cap;
		CharacterInput _input;
		Vector3 _moveDir = Vector3.zero;
		Vector3 _zoneVelocity;
		Vector3 equilibrium;
		float _lookAngle = 90;
		float _lookX = 0;
		float _stepOffset;

		// PROPERTIES
		public CapsuleCollider HitBox => _cap;
		public CollisionBuffer CollisionInfo => _cb;
		public Transform View => cameraSlot;
		public CharacterSoundManager Sound => _sound;
		public int CollisionMask => _layerMask;
		public Vector3 MoveDirection => _moveDir;
		public Vector3 ZoneVelocity => _zoneVelocity;
		public void ResetJumpTimer() => _input.jumpTimer.Restart();
		public float JumpHeight => _input.Crouch ? defaultCrouchJumpHeight : defaultJumpHeight;
		public float MoveSpeed => _input.Walk || _input.Crouch || _cap.height < defaultCrouchHeight
			? defaultWalkSpeed : _input.Sprint ? defaultSprintSpeed : defaultMoveSpeed;
		public Vector3 Gravity { get => _physics.LocalGravity; set => _physics.LocalGravity = value; }
		public Vector3 Velocity { get => _physics.Velocity; set => _physics.Velocity = value; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void DomainInit()
		{
			_layerMask = LayerMask.GetMask(_collisionLayers);
		}

		void Awake()
		{
			// Models
			_guiModel = Models.GetModel<GUIModel>();
			_gameModel = Models.GetModel<GameModel>();
			_cap = gameObject.GetComponent<CapsuleCollider>();
			_character = GetComponent<Character>();
			_cameraRig = GetComponentInChildren<CharacterView>();
			_input = GetComponent<CharacterInput>();
			_damageEvent = GetComponent<DamageEvent>();
			_physics = GetComponent<GravitySampler>();
			_sound = GetComponent<CharacterSoundManager>();
			_cb = new CollisionBuffer(gameObject);

			// Events
			_cb.OnCollision += Impact;
			_character.OnInitialized.AddListener(Initialize);
			_gameModel.isPaused.Subscribe(SetPaused);

			_stepOffset = defaultStepOffset;
			equilibrium = -transform.up;
		}

		void OnEnable()
		{ 
			if (_character.IsPlayer)
				_guiModel.speed.Value = 0f;
		}

		void OnDisable()
		{
			if (_character.IsPlayer)
				_guiModel.speed.Value = 0;
		}

		void OnDestroy()
		{
			_gameModel.isPaused.Unsubscribe(SetPaused);
		}

		void Update()
		{
			_lookX = _input.MouseX;
			lookY = Mathf.Clamp(lookY - _input.MouseY, -_lookAngle, _lookAngle);

			//lookX += cb.AngularVelocityX * Time.deltaTime;
			//lookY -= cb.AngularVelocityY * Time.deltaTime;

			transform.rotation = Quaternion.AngleAxis(_lookX, -equilibrium) * Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, equilibrium), -equilibrium);
			Quaternion yAngle = Quaternion.AngleAxis(lookY, new Vector3(1, 0, 0));

			if (!float.IsNaN(yAngle.w))
				cameraAnchor.localRotation = yAngle;
		}

		void FixedUpdate()
		{
			float dt = Time.fixedDeltaTime;

			_cb.Clear();
			GroundDetection();

			/*  STATE CHANGE ..................................................
				The only thing needed to be counted as grounded is a valid
				floor normal in the collision buffer.                        */
			currentState = _cb.FloorNormal.magnitude != 0 ? MoveState.Grounded : MoveState.Airborne;

			/*  MOVE DIRECTION ................................................
				Pressing the move keys takes you in different directions based
				on move state and collisions.                                */
			if (currentState == MoveState.Grounded)
				_moveDir = Vector3.Cross(transform.right * _input.Forward - transform.forward * _input.Right, _cb.FloorNormal).normalized;
			else if (_cb.Normal.sqrMagnitude == 0)
				_moveDir = Quaternion.FromToRotation(-transform.up, _physics.LocalGravity) * (transform.forward * _input.Forward + transform.right * _input.Right).normalized;
			else
				_moveDir = Quaternion.FromToRotation(-transform.up, falseGravity) * (transform.forward * _input.Forward + transform.right * _input.Right).normalized;

			//if (currentState == MoveState.Grounded /*&& moveDir.sqrMagnitude > 0.5f*/)
			//	GroundMoveCallback?.Invoke();

			/*  FALSE GRAVITY & WALL WALKING ..................................
				FalseGravity is used for orientation and "falling" while
				phys.phys.Gravity is still used for all other physical interactions.
				in normal circumstances, falsGravity == phys.phys.Gravity.             */
			if (!_input.Crawl)
				falseGravity = _physics.LocalGravity;
			else if (
				Physics.SphereCast(transform.position - transform.up * (_cap.height / 2 - _cap.radius), _cap.radius, Vector3.ProjectOnPlane(_moveDir, falseGravity), out RaycastHit hit, _stepOffset * 3, _layerMask, QueryTriggerInteraction.Ignore)
				|| Physics.SphereCast(transform.position, _cap.radius, -transform.up, out hit, _cap.height / 2 - _cap.radius + _stepOffset * 2, _layerMask, QueryTriggerInteraction.Ignore))
			{
				falseGravity = -hit.normal * _physics.LocalGravity.magnitude;

				_cb.AddHit(new CBCollision(hit, _physics.Velocity));
				currentState = MoveState.Grounded;
			}
			else if (_cb.Normal.sqrMagnitude != 0)
				falseGravity = -_cb.Normal * _physics.LocalGravity.magnitude;

			/*  EQUILIBRIUM ...................................................
				This is how the character orients itself to the direction of
				falseGravity. Equilibrium gradulally interpolates toward
				falseGravity. transform.up will always be in the opposite
				direction of equilibrium.                                    */
			if (Vector3.Angle(equilibrium, falseGravity) is float currentAngle && currentAngle != 0)
			{
				float maxAngleDelta = 8f * Mathf.Clamp01(currentAngle / 60f) * dt;
				float faceFactor = (1f + Vector3.Dot(falseGravity.normalized, transform.up)) / 2f;
				Vector3 eqNew = Vector3.RotateTowards(equilibrium, falseGravity, maxAngleDelta, 0);
				eqNew = Vector3.Slerp(eqNew, Vector3.ProjectOnPlane(eqNew, transform.forward).normalized, faceFactor);
				Vector3 yOld = Vector3.ProjectOnPlane(equilibrium, transform.right);
				Vector3 yNew = Vector3.ProjectOnPlane(eqNew, transform.right);
				float yAngle = Vector3.SignedAngle(yOld, yNew, transform.right);
				
				// TODO: Make this selectable
				lookY -= yAngle;

				equilibrium = eqNew.normalized;
			}

			/*  Launch phys.Velocity............................................
				If a platform accelerates downward too quickly, the
				character will be set to airborn and can be
				launched off platforms.                                  */
			if (currentState == MoveState.Grounded)
			{
				float gravityForce = Vector3.Dot(falseGravity, _cb.FloorNormal);
				float pullForce = Vector3.Dot(_cb.PlatformVelocity - _physics.Velocity, _cb.FloorNormal);

				if (pullForce < gravityForce * 0.5f)
					currentState = MoveState.Airborne;
			}

			/*  GROUND MOVEMENT ...............................................
				This is the main walking code. Lots of confusing things going
				on in order to make movement as smooth as possible.          */
			if (currentState == MoveState.Grounded)
			{
				Vector3 relativeVel = _physics.Velocity - _cb.PlatformVelocity;
				Vector3 sideVel = Vector3.ProjectOnPlane(relativeVel, _moveDir);
				Vector3 slopeDir = Vector3.ProjectOnPlane(_moveDir.sqrMagnitude != 0 ? _moveDir : _physics.Velocity.sqrMagnitude != 0 ? _physics.Velocity : transform.forward, transform.up);
				float slopeFactor = (0.5f - Vector3.Angle(slopeDir, _cb.FloorNormal) / 180f);
				float wallFactor = _cb.WallNormal.sqrMagnitude != 0 ? Mathf.Abs(Vector3.Dot(_moveDir, _cb.WallNormal)) : 1;
				float speedFactor = (1f + slopeFactor) * Mathf.Max(1f - wallFactor, wallFactor);
				float topSpeed = MoveSpeed * speedFactor;
				float strideSpeed = defaultStrideSpeed * speedFactor;
				float moveSpeed = Vector3.Dot(relativeVel, _moveDir);
				float sideDec = Mathf.Min(sideVel.magnitude, defaultGroundDecceleration * dt);
				float moveAcc;

				if (moveSpeed <= topSpeed)
				{
					float accRate = (1f + slopeFactor) * dt;

					if (!_input.Sprint || moveSpeed < strideSpeed)
						accRate *= defaultGroundAcceleration;
					else
						accRate *= defaultStrideAcceleration;

					moveAcc = Mathf.Clamp(accRate, 0f, topSpeed - moveSpeed);
				}
				else
					moveAcc = -Mathf.Min(moveSpeed, defaultSpeedDecceleration * dt * (1f - slopeFactor));

				if (moveSpeed < 0)
					moveAcc *= 2;

				_physics.Velocity += _moveDir * moveAcc - sideVel.normalized * sideDec;

				/*  JUMP...................................................
					When a character jumps, the energy required to reach
					jump velocity is transferred into the colliders.     */
				if (_input.Jump)
				{
					float jumpSpeed = Mathf.Sqrt(2f * 9.81f * JumpHeight);
					Vector3 jumpVel = -falseGravity.normalized * jumpSpeed;
					Vector3 verticalVel = Vector3.ProjectOnPlane(relativeVel, falseGravity);
					Vector3 desiredVel = _cb.LimitMomentum(verticalVel + jumpVel, verticalVel, defaultMaxKickVelocity);

					_physics.Velocity = desiredVel + _cb.PlatformVelocity;
					currentState = MoveState.Airborne;
					ResetJumpTimer();
					_cb.AddForce((relativeVel - desiredVel) * defaultMass * 2);
					_sound.PlayJumpSound();
				}

				_sound.PlayFootStepSound();
			}

			/*  AIRBORNE MOVEMENT..............................................
				The character moves differently in the air than while grounded.
				Air acceleration is usually lower than ground acceleration, but
				trying to move backward from your current velocity will 
				increase that acceleration for easier platforming.           */
			if (currentState == MoveState.Airborne)
			{
				Vector3 relativeVel = _physics.Velocity - _zoneVelocity;
				Vector3 verticalVel = Vector3.Project(relativeVel, _physics.LocalGravity);
				Vector3 horizontalVel = relativeVel - verticalVel;
				float accScaleMax = _cb.IsEmpty ? 4f : 0.5f;
				float accScaleMin = _cb.IsEmpty ? 1f : 0.1f;
				float accScaleT = (Vector3.Dot(_moveDir, horizontalVel) + 1) / 2;
				float accScale = Mathf.Lerp(accScaleMax, accScaleMin, accScaleT);
				float moveAcc = defaultAirAcceleration * accScale * dt;
				float moveSpeed = MoveSpeed;
				float currentMoveSpeed = Vector3.Dot(horizontalVel, _moveDir);

				// Clamp move speed.
				if (currentMoveSpeed != 0 && currentMoveSpeed + moveAcc > moveSpeed)
					moveAcc = currentMoveSpeed > moveSpeed ? 0 : currentMoveSpeed + moveAcc - moveSpeed;

				// Keep original excessive speeds, but never exceed it.
				horizontalVel = Vector3.ClampMagnitude(horizontalVel + _moveDir * moveAcc, Mathf.Max(horizontalVel.magnitude, moveSpeed));
				verticalVel += _physics.LocalGravity * dt;
				_physics.Velocity = horizontalVel + verticalVel + _zoneVelocity;

				if (_input.Jump)
					OnWallJump?.Invoke(this);

				if (_input.Glide)
					OnGlide?.Invoke(this);
			}

			OverlapFix();
			Move();

			if (_character.IsPlayer)
				_guiModel.speed.Value = Mathf.Lerp(_guiModel.speed.Value, Vector3.ProjectOnPlane(Velocity, Gravity).magnitude * 10, 0.2f);
		}

		void Initialize(bool isPlayer)
		{
			if (thirdPersonBody)
				thirdPersonBody.SetActive(!isPlayer);
			if (cameraAnchor && cameraAnchor.TryGetComponent(out MeshRenderer mr))
				mr.enabled = !isPlayer;
		}

		void SetPaused(DeltaValue<bool> paused)
		{
			enabled = !paused.newValue;
		}

		void GroundDetection()
		{
			Vector3 position = transform.position;
			Vector3 up = transform.up;
			float radius = _cap.radius;
			float capDistance = _cap.height / 2f - radius - INSET;
			Vector3 rayPoint = position - up * capDistance;
			float rayDistanceScale = currentState == MoveState.Grounded ? 2 : 1;
			float rayDistance = _stepOffset * rayDistanceScale - INSET;

			if (Physics.SphereCast(rayPoint, radius * 0.9f, -up, out RaycastHit hit, rayDistance, _layerMask, QueryTriggerInteraction.Ignore))
			{
				CBCollision collision = new CBCollision(hit, _physics.Velocity);

				// Step Override
				rayPoint = position - up * (_cap.height / 2f);

				if (Physics.Raycast(rayPoint, -up, out hit, rayDistance, _layerMask, QueryTriggerInteraction.Ignore)
					|| Physics.Raycast(rayPoint + transform.forward * radius / 2f, -up, out hit, rayDistance, _layerMask, QueryTriggerInteraction.Ignore))
					collision.normal = hit.normal;

				// Pull Force
				Vector3 groundVelocity = collision.rigidbody ? collision.rigidbody.velocity : Vector3.zero;
				float gravityForce = Vector3.Dot(falseGravity, collision.normal);
				float pullForce = Vector3.Dot(groundVelocity - _physics.Velocity, collision.normal);

				if (pullForce >= gravityForce * 0.5f)
				{
					// Step Offset
					float trigOpposite = Vector3.ProjectOnPlane(position - collision.point, up).magnitude;
					float trigAdjacent = new Vector2(radius, trigOpposite).magnitude;
					float floorDelta = collision.distance - _stepOffset - INSET + trigAdjacent - radius;

					transform.position -= up * floorDelta;
					_cameraRig.stepOffset = Mathf.Clamp(_cameraRig.stepOffset + floorDelta, -0.5f, 0.5f);
					_cb.AddHit(collision);
				}
			}
		}

		void OverlapFix()
		{
			Quaternion rotation = transform.rotation;
			Vector3 up = transform.up;
			Vector3 position = transform.position;
			Vector3 oldPos = position;
			Vector3 finalOffset = Vector3.zero;
			Vector3 capPoint = up * (_cap.height * 0.5f - _cap.radius);
			Vector3 point0 = position + capPoint;
			Vector3 point1 = position - capPoint;
			int count = Physics.OverlapCapsuleNonAlloc(point0, point1, _cap.radius, _colliderBuffer, _layerMask, QueryTriggerInteraction.Ignore);

			for (int i = 0; i < count; i++)
			{
				Collider collider = _colliderBuffer[i];

				if (!collider.transform.IsChildOf(transform)
					&& Physics.ComputePenetration(_cap, position, rotation,
						collider, collider.transform.position, collider.transform.rotation,
						out Vector3 direction, out float distance))
				{
					Vector3 normal = direction;
					direction *= distance;
					Vector3 fpd = finalOffset + direction;

					//horizontal squeeze prevention
					if (Vector3.Dot(up, normal) < 0 && _cb.FloorNormal.sqrMagnitude != 0)
						normal = Vector3.ProjectOnPlane(normal, up).normalized;

					// Vertical squeeze prevention
					if (Vector3.Dot(finalOffset, direction) < 0 && Vector3.Dot(_physics.Velocity, fpd) < 0)
						_physics.Velocity = Vector3.Project(_physics.Velocity, Vector3.Cross(finalOffset, direction));

					finalOffset = fpd;

					_cb.AddHit(new CBCollision(collider, normal, position, _physics.Velocity));
					position += direction;
				}
			}

			Vector3 dir = position - oldPos;
			float squeeze = dir.magnitude;
			int damage = (int)(squeeze * 200);
			GameObject instigatorBody = _colliderBuffer[0] ? _colliderBuffer[0].gameObject : null;
			CharacterInfo instigator;

			// Instigated by Character or self-instigated
			if (instigatorBody && instigatorBody.TryGetComponent(out Character ch) && ch.Info)
				instigator = ch.Info;
			else
				instigator = _character.Info;

			if (squeeze > _cap.radius)
				_damageEvent.Damage(damage, instigatorBody, instigator, impactDamageType, dir);

			transform.position = position;
		}

		void Move()
		{
			float dt = Time.fixedDeltaTime;
			float iterations = 3;
			float backup = _cap.radius * 0.5f;
			Vector3 pointOffset = transform.up * (_cap.height / 2f - _cap.radius);

			while (_physics.Velocity.sqrMagnitude > 0 && dt > 0 && iterations-- > 0)
			{
				float distance = _physics.Velocity.magnitude * dt;
				Vector3 velOff = -_physics.Velocity.normalized * backup;
				Vector3 capCenter = transform.TransformPoint(_cap.center);

				if (Physics.CapsuleCast(
							point1: capCenter + velOff + pointOffset,
							point2: capCenter + velOff - pointOffset,
							radius: _cap.radius,
							direction: _physics.Velocity,
							maxDistance: distance + backup,
							hitInfo: out RaycastHit hit,
							layerMask: _layerMask,
							queryTriggerInteraction: QueryTriggerInteraction.Ignore)
					&& !hit.transform.IsChildOf(transform)
					&& hit.distance > backup)
				{
					hit.distance -= backup;
					distance = Mathf.Min(hit.distance, distance);
					_cb.AddHit(new CBCollision(hit, _physics.Velocity));
				}

				transform.Translate(Vector3.ClampMagnitude(_physics.Velocity, distance), Space.World);

				dt -= distance / _physics.Velocity.magnitude;
			}
		}

		public void Impact(CBCollision hit, float impactSpeed)
		{

			int damage = (int)(Mathf.Pow(impactSpeed - defaultMaxSafeImpactSpeed, 1.5f) * 2.5f);
			CharacterInfo instigator;

			if (hit.gameObject.TryGetComponent(out Character ch) && ch.Info)
				instigator = ch.Info;
			else
				instigator = _character.Info;

			if (damage > fallForgiveness)
				_damageEvent.Damage(damage, hit.gameObject, instigator, impactDamageType, hit.normal);

			_sound.PlayImpactSound(-hit.normal, impactSpeed);
		}

		public void ScaleBody(float mult)
		{
			_cap.radius *= mult;
			_cap.height *= mult;

			defaultSprintSpeed *= mult;
			defaultMoveSpeed *= mult;
			defaultStrideSpeed *= mult;
			defaultWalkSpeed *= mult;
			defaultJumpHeight *= mult;
			defaultCrouchJumpHeight *= mult;
			defaultStepOffset *= mult;
			defaultAirAcceleration *= mult;
			defaultGroundAcceleration *= mult;
			defaultStrideAcceleration *= mult;
			defaultSpeedDecceleration *= mult;
			defaultGroundDecceleration *= mult;
			defaultCrouchHeight *= mult;
			defaultHeight *= mult;
			_stepOffset *= mult;
			thirdPersonBody.transform.localScale *= mult;
			cameraAnchor.transform.localScale *= mult;
			defaultMaxSafeImpactSpeed *= mult;
			if (gameObject.TryGetComponentInChildren(out CharacterCameraEyeOffset cc))
				cc.eyeOffset *= mult;
		}
	}
}
