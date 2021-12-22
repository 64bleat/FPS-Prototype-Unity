using MPWorld;
using System;
using UnityEngine;

namespace MPCore
{
	/// <summary> 
	///     Superclass for all inventory items. 
	/// </summary>
	public class Inventory : ScriptableResource
	{
		[Header("Inventory")]
		public InventoryPickup dropPrefab;
		[Tabulate("Name")]
		public string displayName;
		[Tabulate("Count")]
		public int count = 1;
		public int maxCount = 1;
		public bool staticReference = false;
		public bool pickupOnTouch = false;
		public bool pickupOnInteractStart = true;
		public bool destroyOnPickup = false;
		public bool destroyOnDrop = false;
		public bool dropOnDeath = true;
		public float droppedLifeTime = 30f;
		public AudioClip pickupSound;
		public bool isCopy = false;
		public bool activatable = false;
		public bool active = false;

		[NonSerialized] public Inventory asset;

		void Awake()
		{
			asset = this;
		}

		[Tabulate("Id")]
		public string ID => GetInstanceID().ToString();

		/// <summary> Try to drop an inventory item </summary>
		/// <returns> true if the item was successfully dropped </returns>

		public bool SetActive(GameObject owner, bool active)
		{
			if(this.active != active)
				if(active)
					OnActivate(owner);
				else
					OnDeactivate(owner);

			return this.active = active;
		}

		public virtual void OnActivate(GameObject owner)
		{

		}

		public virtual void OnDeactivate(GameObject owner)
		{

		}

		/// <summary> Child-defined code for approving pick-ups </summary>
		/// <remarks> OnPickup is essentially the OnEnable of Inventory.
		/// It is called before the item is copied for pickup. </remarks>
		/// <param name="picker"> The active gameObject that is trying to pick this item up </param>
		/// <returns> true when the pick-up is approved </returns>
		public virtual bool TryPickup(GameObject picker)
		{
			return true;
		}

		/// <summary> Child-defined code for approving drops </summary>
		/// <remarks> OnDrop is essentially the OnDestroy of Inventory.
		/// It is called before the item is dropped. </remarks>
		/// <param name="owner"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns> true when the drop is approved </returns>
		public virtual bool TryDrop(GameObject owner, Vector3 position, Quaternion rotation)
		{
			return true;
		}
	}
}
