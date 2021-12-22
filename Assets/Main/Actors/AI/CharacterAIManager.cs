using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MPCore
{
	public class CharacterAIManager : MonoBehaviour
	{
		static readonly string[] VIEW_LAYERS = { "Default", "Physical", "Player" };
		static int VIEW_MASK;

		// Stats
		DataValue<bool> hostile = new(true);
		DataValue<float> viewAngle = new(45);
		DataValue<float> viewDistance = new(100);
		DataValue<float> scoutingTime = new(0.5f);
		DataValue<float> pickTargetTime = new(0.5f);
		DataValue<float> certaintyDecay = new(5f);
		DataValue<float> stubbornness = new(1.5f);
		DataValue<float> accuracy = new(0.6f);
		DataValue<float> maxInaccuracyDistance = new(5f);
		DataValue<float> mouseSpeed = new(420f);
		DataValue<float> mouseAcceleration = new(45f);

		//References
		Character _character;
		CharacterBody _body;
		InventoryManager _inventory;
		InputManager _input;
		WeaponSwitcher _weapons;
		int _id;

		// Layer 0: Sensory: World -> Memory
		AIModel _aiModel;
		readonly DataValue<Coroutine> _attackTargetSightCoroutine = new();
		readonly DataValue<Coroutine> _pickupTargetSightCoroutine = new();
		// Layer 1: Discernment: Memory -> Objective
		readonly List<AttackMemory> _attackTargetMemory = new();
		readonly List<ItemMemory> _pickupTargetMemory = new();
		readonly DataValue<Coroutine> _attackTargetPickCoroutine = new();
		readonly DataValue<Coroutine> _pickupTargetPickCoroutine = new();
		// Layer 2: Execution: Objective -> Action
		readonly DataValue<AttackMemory> _attackTarget = new();
		readonly DataValue<ItemMemory> _pickupTarget = new();
		// Layer 3: Ability: Action -> Game
		readonly DataValue<Vector3?> _lookPosition = new();
		readonly DataValue<Vector3?> _movePosition = new();

		float projectileSpeed;
		Vector3 _accuracyOffset = Vector3.zero;

		class Memory
		{
			public float certainty;
			public float priority;
			public float lastKnownTime;
			public Vector3 lastKnownPosition;
			public Vector3 lastKnownVelocity;
		}

		class AttackMemory : Memory
		{
			public Character character;
			public Weapon lastKnownWeapon;
		}

		class ItemMemory : Memory
		{
			public InventoryPickup item;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void DomainInit()
		{
			VIEW_MASK = LayerMask.GetMask(VIEW_LAYERS);
		}

		void Awake()
		{
			_aiModel = Models.GetModel<AIModel>();
			_character = GetComponent<Character>();
			_body = GetComponent<CharacterBody>();
			_inventory = GetComponent<InventoryManager>();
			_input = GetComponent<InputManager>();
			_weapons = GetComponent<WeaponSwitcher>();

			_id = gameObject.GetInstanceID() % 4016;

			_character.OnInitialized.AddListener(Initialize);

			// Layer 0
			_attackTargetSightCoroutine.Subscribe(SwitchCoroutine);
			_pickupTargetSightCoroutine.Subscribe(SwitchCoroutine);
			// Layer 1
			_attackTargetPickCoroutine.Subscribe(SwitchCoroutine);
			_pickupTargetPickCoroutine.Subscribe(SwitchCoroutine);
			// Layer 2
		}

		void Start()
		{
			// Layer 0
			_attackTargetSightCoroutine.Value = StartCoroutine(SightAttackTargets(scoutingTime));
			_pickupTargetSightCoroutine.Value = StartCoroutine(SightPickupTargets(scoutingTime));
			// Layer 1
			_attackTargetPickCoroutine.Value = StartCoroutine(PickAttackTarget(pickTargetTime));
			_pickupTargetPickCoroutine.Value = StartCoroutine(PickPickupTarget(pickTargetTime));
		}

		private void OnEnable()
		{
			_input.OnMouseMove += OnMouseMove;
		}

		private void OnDisable()
		{
			_input.OnMouseMove -= OnMouseMove;
		}

		void Initialize(bool isPlayer)
		{
			enabled = isPlayer;

			if(isPlayer)
			{
				GUIModel gui = Models.GetModel<GUIModel>();
				_attackTarget.Subscribe(dv => { 
					if(dv.newValue != dv.oldValue)
						gui.largeMessage.Value = $"You want to attack { (dv.newValue != null && dv.newValue.character ? dv.newValue.character.name : "Nothing")}"; });
				_pickupTarget.Subscribe(dv => {
				if (dv.newValue != dv.oldValue)
					gui.shortMessage.Value = $"You want to pickup {(dv.newValue != null && dv.newValue.item ? dv.newValue.item.name : "Nothing")}";});
			}
		}

		#region Layer 1
		/// <summary>
		/// Sight targets and register them to memory
		/// </summary>
		IEnumerator SightAttackTargets(float waitTime)
		{
			foreach (Character character in _aiModel.characters)
			{
				if (IsVisible(character, out RaycastHit hit))
				{
					AttackMemory memory = _attackTargetMemory.Find(mem => mem.character == character);

					if (memory == null)
						_attackTargetMemory.Add(memory = new());

					memory.character = character;
					memory.certainty = Mathf.Clamp01(memory.certainty + 1f);
					memory.lastKnownTime = Time.time;
					memory.lastKnownPosition = hit.point;
					memory.lastKnownWeapon = character.Weapons.currentWeapon;
				}
			}

			// Restart
			yield return new WaitForSeconds(waitTime);

			_attackTargetSightCoroutine.Value = StartCoroutine(SightAttackTargets(scoutingTime));
		}

		IEnumerator SightPickupTargets(float waitTime)
		{
			foreach (InventoryPickup item in _aiModel.pickups)
			{
				RaycastHit hit = default;
				hit.point = item.transform.position;

				if (item.IsAIGlobal || IsVisible(item, out hit))
				{
					ItemMemory memory = _pickupTargetMemory.Find(mem => mem.item == item);

					if (memory == null)
						_pickupTargetMemory.Add(memory = new());

					memory.item = item;
					memory.certainty = Mathf.Clamp01(memory.certainty + 1f);
					memory.lastKnownTime = Time.time;
					memory.lastKnownPosition = hit.point;
				}
			}

			yield return new WaitForSeconds(waitTime);

			StartCoroutine(SightPickupTargets(scoutingTime));
		}
		#endregion
		#region Layer 2
		/// <summary>
		/// Decite which character you want to attack
		/// Remove old memories from memroy
		/// </summary>
		IEnumerator PickAttackTarget(float waitTime)
		{
			float GetPriorityLevel(AttackMemory mem)
			{
				float distance = Vector3.Distance(_body.View.position, mem.lastKnownPosition);
				float dFactor = Mathf.Pow(1.05f, -distance);
				float stubFactor = mem == _attackTarget.Value ? stubbornness.Value : 1f;
				return dFactor * stubFactor * mem.certainty;
			}

			// Update Memories
			_attackTargetMemory.RemoveAll(mem => !mem.character);

			foreach(var mem in _attackTargetMemory)
				if (IsVisible(mem.character, out RaycastHit hit))
				{
					mem.certainty = Mathf.Clamp01(mem.certainty + 1f);
					mem.lastKnownPosition = hit.point;
					mem.lastKnownTime = Time.time;
					mem.lastKnownWeapon = mem.character.Weapons.currentWeapon;
					mem.lastKnownVelocity = mem.character.Body.Velocity;
				}
				else
					mem.certainty -= waitTime / certaintyDecay;

			_attackTargetMemory.RemoveAll(mem => mem.certainty <= 0);

			foreach (AttackMemory mem in _attackTargetMemory)
				mem.priority = GetPriorityLevel(mem);


			// Pick Target
			AttackMemory target = _attackTargetMemory
				.Where(mem => mem.priority > 0)
				.OrderByDescending(mem => mem.priority)
				.FirstOrDefault();

			if (target != null)
				_attackTarget.Value = target;
			else
				_attackTarget.Value = null;

			// Restart
			yield return new WaitForSeconds(waitTime);

			_attackTargetPickCoroutine.Value = StartCoroutine(PickAttackTarget(pickTargetTime));
		}

		IEnumerator PickPickupTarget(float waitTime)
		{
			// Update Memory
			float GetPriorityLevel(ItemMemory mem)
			{
				float distance = Vector3.Distance(mem.lastKnownPosition, _body.View.position);
				float dFactor = Mathf.Pow(1.1f, -distance);
				ResourceValue hp = _character.Health;
				int needHealth = Mathf.Max(0, hp.MaxValue - hp.Value);
				float percentHealth = (float)needHealth / Mathf.Max(1, hp.MaxValue);
				bool has = _character.Inventory.Inventory.Find(item => item.resourcePath == mem.item.inventory.resourcePath);
				float hasFactor = has ? 0f : 1f;
				float stubFactor = mem == _pickupTarget.Value ? stubbornness.Value : 1f;

				if (mem.item.inventory is HealthPickup health)
				{
					float fill = Mathf.Clamp01((float)needHealth / Mathf.Max(1, health.RestoreAmount));

					return percentHealth * dFactor * stubFactor * mem.certainty;
				}
				else if (mem.item.inventory is Weapon weapon)
				{
					float oppHealth = 1f - percentHealth;

					return hasFactor * dFactor * oppHealth * stubFactor * mem.certainty;
				}

				return hasFactor * dFactor * 0.1f * stubFactor * mem.certainty;
			}

			_pickupTargetMemory.RemoveAll(mem => !mem.item);

			foreach (var mem in _pickupTargetMemory)
				if (mem.item.IsAIGlobal)
				{
					mem.certainty = 1f;
					mem.lastKnownPosition = mem.item.transform.position;
					mem.lastKnownTime = Time.time;
				}
				else if (IsVisible(mem.item, out RaycastHit hit))
				{
					mem.certainty = Mathf.Clamp01(mem.certainty + 1f);
					mem.lastKnownPosition = hit.point;
					mem.lastKnownTime = Time.time;
				}
				else
					mem.certainty -= waitTime / certaintyDecay;

			_pickupTargetMemory.RemoveAll(mem => mem.certainty <= 0);

			foreach (ItemMemory mem in _pickupTargetMemory)
				mem.priority = GetPriorityLevel(mem);

			// Pick Pickup
			ItemMemory target = _pickupTargetMemory
				.Where(mem => mem.priority > 0f)
				.OrderByDescending(mem => mem.priority)
				.FirstOrDefault();

			if (target != null)
				_pickupTarget.Value = target;
			else
				_pickupTarget.Value = null;

			yield return new WaitForSeconds(waitTime);

			StartCoroutine(PickPickupTarget(pickTargetTime));
		}
		#endregion
		#region Layer 3
		Vector2 OnMouseMove(float dt)
		{
			float tick = Time.frameCount;
			bool characterValid = _attackTarget.Value?.character != null;
			Vector3 focalPoint;
			Vector3 targetVelocity;

			// Get Attack Focal Point
			if (characterValid)
			{
				if (Mathf.Approximately(_attackTarget.Value.certainty, 1f))
					focalPoint = _attackTarget.Value.character.Body.View.position;
				else
				{
					float lastSeen = Time.time - _attackTarget.Value.lastKnownTime;
					Vector3 direction = _attackTarget.Value.lastKnownVelocity * lastSeen;
					Ray ray = new Ray(_attackTarget.Value.lastKnownPosition, direction);

					if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude, VIEW_MASK, QueryTriggerInteraction.Ignore))
						focalPoint = hit.point;
					else
						focalPoint = _attackTarget.Value.lastKnownPosition + direction;
				}

				targetVelocity = _attackTarget.Value.lastKnownVelocity;
			}
			 // Get Idle Focal Point
			else
			{
				Vector3 velocityY = Vector3.ProjectOnPlane(_body.Velocity, _body.transform.right);
				velocityY *= Mathf.Max(Mathf.Sign(Vector3.Dot(_body.Velocity, _body.transform.forward)), 0);
				focalPoint = _body.View.position + _body.transform.forward * 3f + velocityY;
				targetVelocity = Vector3.zero;
			}

			// Shared Values
			float targetDistance = Vector3.Distance(_body.View.position, focalPoint);

			// Velocity - all done!

			//// Weapon Switch
			//if(tick % 120 == 0 && character)
			//{
			//	Weapon theirs = character.Weapons.currentWeapon;

			//	if(theirs && theirs.resourcePath == "Knife")
			//	{
			//		_weapons.DrawWeapon(theirs.weaponSlot);
			//	}
			//	else
			//	{
			//		Weapon best = _inventory.Inventory
			//			.Select(item => item as Weapon)
			//			.Where(wep => wep)
			//			.OrderBy(wep => Mathf.Abs(wep.preferredCombatDistance - targetDistance))
			//			.FirstOrDefault();

			//		_weapons.DrawWeapon(best);
			//	}
			//}

			// Projectile Speed;
			float projectileSpeed;
			Vector3 projectileOffset;

			if (_weapons.currentWeapon
				&& _weapons.currentWeapon.projectilePrimary
				&& _weapons.currentWeapon.projectilePrimary.TryGetComponent(out Projectile p))
				projectileSpeed = p.shared.exitSpeed;
			else
				projectileSpeed = 50000f;

			// Extrapolate Velocity
			projectileOffset = targetVelocity * targetDistance / projectileSpeed;
			
			// Recalculate Accuracy
			//if(tick % 60 == 0)
			//{
			//	Vector3 viewVelocity = Vector3.ProjectOnPlane(targetVelocity - _body.Velocity, _body.View.forward);
			//	float vScale = Mathf.Clamp01(viewVelocity.magnitude / _body.defaultSprintSpeed);
			//	_accuracyOffset = Random.insideUnitSphere * (1f - accuracy) * vScale;
			//}

			focalPoint += projectileOffset;
			focalPoint += _accuracyOffset * Mathf.Min(maxInaccuracyDistance, targetDistance);

			// Get Look Direction
			Vector3 desiredDir = focalPoint - _body.View.position;
			float offsetAngle = Vector3.Angle(desiredDir, _body.View.forward);

			// Combat
			if (tick % 15 == 0
				&& characterValid
				&& hostile.Value
				&& _attackTarget.Value.certainty > 0.5f
				&& _weapons.currentWeapon
				&& _weapons.currentWeapon.validFireAngle >= offsetAngle
				&& _weapons.currentWeapon.engagementRange >= desiredDir.magnitude)
				_input.BotKeyDown("fire", 0.5f);

			// MouseLook
			Vector3 bodyUp = _body.transform.up;
			Vector3 bodyRight = _body.transform.right;
			Vector3 bodyForward = _body.transform.forward;
			Vector3 viewForward = _body.View.forward;
			Vector3 desiredDirX = Vector3.ProjectOnPlane(desiredDir, bodyUp);
			Vector3 desiredDirY = Vector3.ProjectOnPlane(desiredDir, bodyRight);
			float deltaAngle = Vector3.Angle(viewForward, desiredDir);
			float deltaX = Vector3.SignedAngle(bodyForward, desiredDirX, bodyUp);
			float deltaY = Vector3.SignedAngle(viewForward, desiredDirY, bodyRight);
			float mouseScale = mouseSpeed * Mathf.Clamp01(deltaAngle / mouseAcceleration);
			Vector2 deltaMouse = new Vector2(deltaX, -deltaY);

			return deltaMouse.normalized * mouseScale * dt;
		}


		#endregion

		bool IsVisible(Component target, out RaycastHit hit)
		{
			Collider targetCollider = target.GetComponent<Collider>();

			hit = default;
			hit.point = target.transform.position;

			// No Collider Test
			if (!targetCollider)
			{
				Vector3 targetPosition = target.transform.position;
				Vector3 targetOffset = targetPosition - _body.View.position;
				float targetDistance = Vector3.Distance(target.transform.position, _body.View.position);

				// Distance Test
				if (targetDistance > viewDistance)
					return false;

				// AngleTest
				float targetAngle = Vector3.Angle(targetOffset, _body.View.forward);

				if (targetAngle > viewAngle)
					return false;

				// RaycastTest
				Ray ray = new Ray(_body.View.position, targetOffset);
				bool rayHit = Physics.Raycast(ray, out hit, targetDistance, VIEW_MASK, QueryTriggerInteraction.Ignore);

				if (rayHit)
					return hit.collider.transform.IsChildOf(target.transform);

				// Exhausted Tests
				return true;
			}
			else // Collider Tests
			{
				float targetboundExtent = targetCollider.bounds.max.magnitude;
				Vector3 randPosition = targetCollider.transform.TransformPoint(Random.onUnitSphere * targetboundExtent);
				Vector3 targetPosition = targetCollider.ClosestPoint(randPosition);
				Vector3 targetOffset = targetPosition - _body.View.position;
				float targetDistance = targetOffset.magnitude;

				// Distance Test
				if (targetDistance > viewDistance)
					return false;

				// Angle Test
				float targetAngle = Vector3.Angle(targetOffset, _body.View.forward);

				if (targetAngle > viewAngle)
					return false;

				// Raycast Test
				Ray ray = new Ray(_body.View.position, targetOffset);

				if (Physics.Raycast(ray, out hit, targetDistance * 1.05f, VIEW_MASK, QueryTriggerInteraction.Ignore))
					return hit.collider == targetCollider;

				// Exhausted Tests
				return false;
			}
		}

		void SwitchCoroutine(DeltaValue<Coroutine> coroutine)
		{
			if (coroutine.oldValue != null)
				StopCoroutine(coroutine.oldValue);
		}
	}
}
