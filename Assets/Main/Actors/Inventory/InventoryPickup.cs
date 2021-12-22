using MPWorld;
using UnityEngine;

namespace MPCore
{
	public class InventoryPickup : MonoBehaviour, IInteractable, ITouchable
	{
		[ContextCreateAsset] 
		public Inventory inventory;
		public bool countDownDestroy = false;
		public float lifeTime;
		public bool destroyOnPickup = true;
		[SerializeField] bool _aiGlobal = false;

		AIModel _aiModel;

		public bool IsAIGlobal => _aiGlobal;

		void Awake()
		{
			_aiModel = Models.GetModel<AIModel>();
		}

		private void OnEnable()
		{
			_aiModel.pickups.Add(this);
			AIBlackboard.mentalTargets.Add(this);
		}

		private void OnDisable()
		{
			_aiModel.pickups.Remove(this);
			AIBlackboard.mentalTargets.Remove(this);
		}

		public void Update()
		{
			if (countDownDestroy && (lifeTime -= Time.deltaTime) <= 0)
				Destroy(gameObject);
		}

		public virtual void OnPickup(GameObject picker)
		{
			if (picker && picker.TryGetComponent(out InventoryManager container)
				&& container.TryPickup(inventory, out _))
			{
				gameObject.SetActive(false);

				if (inventory.pickupSound && picker.TryGetComponent(out CharacterSoundManager sound))
					sound.PlayPickupSound(inventory.pickupSound);

				if(destroyOnPickup)
					Destroy(gameObject);
			}
		}

		public virtual void OnDropped(GameObject dropper)
		{
			//if (inventory.droppedLifeTime > 0)
			//{
			//    countDownDestroy = true;
			//    lifeTime = inventory.droppedLifeTime;
			//}
		}
		
		public virtual void OnTouch(GameObject instigator, Collision c)
		{
			if (inventory && inventory.pickupOnTouch && instigator)
				OnPickup(instigator);
		}

		private void OnTriggerEnter(Collider other)
		{
			if(inventory && inventory.pickupOnTouch && other)
				OnPickup(other.gameObject);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (inventory && inventory.pickupOnTouch && collision != null)
				OnPickup(collision.gameObject);
		}

		public void OnInteractStart(GameObject other, RaycastHit hit)
		{
			if (inventory && inventory.pickupOnInteractStart && other)
				OnPickup(other);
		}
		public void OnInteractEnd(GameObject other, RaycastHit hit) { }
		public void OnInteractHold(GameObject other, RaycastHit hit) { }
	}
}
