using MPConsole;
using MPWorld;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
	[ContainsConsoleCommands]
	public class InventoryManager : MonoBehaviour
	{
		[SerializeField] List<Inventory> _inventory;

		GUIModel _guiModel;
		Character _character;
		IGravityUser _physics;
		Rigidbody _rigidbody;

		public IList<Inventory> Inventory => _inventory;

		void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_physics = GetComponent<IGravityUser>();
			_guiModel = Models.GetModel<GUIModel>();
			_character = GetComponent<Character>();

			// Don't store direct references to Inventory resources 
			for (int i = 0; i < _inventory.Count; i++)
				if (!_inventory[i].staticReference)
					_inventory[i] = Instantiate(_inventory[i]);

			Console.AddInstance(this);
		}

		void OnDestroy()
		{
			Console.RemoveInstance(this);
		}

		public bool TryPickup(Inventory reference, out Inventory instance)
		{
			// Set instance
			if (reference.destroyOnPickup || reference.staticReference || reference.isCopy)
				instance = reference;
			else
			{
				instance = Instantiate(reference);
				instance.asset = reference.asset;
				instance.isCopy = true;
			}

			// Pickup DestroyOnPickup
			if (reference.destroyOnPickup)
				return Pickup(instance);

			// Pickup Duplicate
			foreach (Inventory item in _inventory)
				if(item.resourcePath == reference.resourcePath)
					if (item.count >= item.maxCount)
						return false;
					else if (Pickup(instance))
					{
						item.count = Mathf.Min(item.maxCount, item.count + reference.count);
						return true;
					}
					else
						return false;

			if (Pickup(instance))
			{
				_inventory.Add(instance);

				return true;
			}
			else if (instance != this)
				Destroy(instance);

			return false;
		}

		bool Pickup(Inventory instance)
		{
			bool passed = instance.TryPickup(gameObject);

			// Activatables
			if (instance.activatable)
			{
				instance.active = false;
				instance.SetActive(gameObject, true);
			}

			// Display Pickup Message
			if (passed && _character && _character.IsPlayer)
			{
				_guiModel.shortMessage.Value = $"Acquired {instance.displayName}";

				if (instance.activatable)
					_guiModel.PassivePickup?.Invoke(instance);
			}

			return passed;
		}

		/// <summary> Players pick up an item using the console </summary>
		/// <param name="resourcePath"></param>
		/// <returns></returns>
		[ConsoleCommand("pickup")]
		public string ConsolePickup(string resourcePath)
		{
			if (_character ? _character.IsPlayer : false)
			{
				Inventory resource = Resources.Load<Inventory>(resourcePath);

				if (resource)
					if(TryPickup(resource, out _))
						return $"Picked up {resource.name}";
					else
						return $"Could not pick up {resource.name}";
				else
					return $"Could not find resource {resourcePath}";
			}
			else
				return null;
		}

		public bool TryDrop(Inventory item, Vector3 position, Quaternion rotation, RaycastHit dropPoint, out InventoryPickup pickup)
		{
			pickup = null;

			if (item.TryDrop(gameObject, position, rotation) 
				&& !item.destroyOnDrop 
				&& item.dropPrefab)
			{
				// Get Velocity
				Vector3 ownVel;

				if (_physics != null)
					ownVel = _physics.Velocity;
				else if (_rigidbody)
					ownVel = _rigidbody.velocity;
				else
					ownVel = Vector3.zero;

				// Deactivate if Active
				item.SetActive(gameObject, false);

				// Remove In HUD
				if (item.activatable && (_character ? _character.IsPlayer : false))
					_guiModel.PassiveDrop?.Invoke(item);

				for (int i = 0, count = item.count; i < count; i++)
				{
					// Make Pickup
					pickup = Instantiate(item.dropPrefab, position, rotation);

					pickup.inventory =  i > 0 ? Instantiate(item) : item;
					pickup.inventory.count = 1;
					pickup.countDownDestroy = item.droppedLifeTime > 0;
					pickup.lifeTime = item.droppedLifeTime;

					// Set Velocity
					if (pickup.TryGetComponent(out Rigidbody newRB))
						newRB.velocity += ownVel;
					else if (pickup.TryGetComponent(out IGravityUser newGU))
						newGU.Velocity += ownVel;

					// Clipping prevention
					if (pickup.TryGetComponent(out Collider collider))
						pickup.transform.position += collider.transform.position - collider.ClosestPoint(collider.transform.position - dropPoint.normal * 100);
				}
			}
			else
				pickup = null;

			return !item || !item.dropPrefab || item.destroyOnDrop || pickup;
		}

		public InventoryPickup Drop(Inventory item, Vector3 position, Quaternion rotation, RaycastHit hit)
		{
			if (TryDrop(item, position, rotation, hit, out InventoryPickup pickup))
			{
				_inventory.Remove(item);
				pickup.OnDropped(gameObject);
			}

			return pickup;
		}

		public InventoryPickup Drop(int index, Vector3 position, Quaternion rotation, RaycastHit hit)
		{
			if(TryDrop(_inventory[index], position, rotation, hit, out InventoryPickup pickup))
			{
				_inventory.RemoveAt(index);
				pickup.OnDropped(gameObject);
			}

			return pickup;
		}

		/// <summary> Find an instance of an item within this container </summary>
		/// <param name="reference"></param>
		/// <param name="instance"></param>
		/// <returns> true if a clone was found </returns>
		public bool TryFind(Inventory reference, out Inventory instance)
		{
			try
			{
				instance = _inventory.Find(i => i.asset == reference.asset);
				return instance;
			}
			catch
			{
				instance = null;
				return false;
			}
		}

		public void Remove(Inventory reference, int count = 1)
		{
			if (TryFind(reference, out Inventory item))
				if ((item.count -= count) <= 0)
					_inventory.Remove(item);
		}
	}
}
