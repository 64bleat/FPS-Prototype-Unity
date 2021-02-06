using MPWorld;
using UnityEngine;

namespace MPCore
{
    /// <summary> 
    ///     Superclass for all inventory items. 
    /// </summary>
    public class Inventory : ScriptableResource
    {
        [Header("Inventory")]
        public GameObject sceneObjectPrefab;
        [GUITableValue("Name")]
        public string displayName;
        [GUITableValue("Count")]
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

        [GUITableValue("Id")]
        public string ID => GetInstanceID().ToString();

        [GUIContextMenuOption("TestLog")]
        public void TestLog()
        {
            Debug.Log($"{displayName} performed a test log at {Time.time}");
        }

        /// <summary> Pickup the item and call OnPickup, returns true if pickup was a success. </summary>
        /// 
        public bool TryPickup(InventoryContainer container, out Inventory instance)
        {
            // Set instance
            if (destroyOnPickup || staticReference || isCopy)
                instance = this;
            else
            {
                instance = Instantiate(this);
                instance.isCopy = true;
            }

            // Pickup DestroyOnPickup
            if (destroyOnPickup)
                return instance.OnPickupInternal(container);

            // Pickup Duplicate
            foreach (Inventory item in container.inventory)
                if (item.displayName.Equals(displayName))
                    if (item.count >= item.maxCount)
                        return false;
                    else if (instance.OnPickupInternal(container))
                    {
                        item.count = Mathf.Min(item.maxCount, item.count + count);
                        return true;
                    }
                    else
                        return false;

            if (instance.OnPickupInternal(container))
            {
                container.inventory.Add(instance);

                return true;
            }
            else if (instance != this)
                Destroy(instance);

            return false;
        }

        /// <summary> Try to drop an inventory item </summary>
        /// <returns> true if the item was successfully dropped </returns>
        public bool TryDrop(GameObject owner, Vector3 position, Quaternion rotation, RaycastHit dropPoint, out GameObject droppedObject)
        {
            if (OnDrop(owner, position, rotation) && !destroyOnDrop && sceneObjectPrefab)
            {
                // Deactivate if Active
                SetActive(owner, false);

                droppedObject = Instantiate(sceneObjectPrefab, position, rotation);

                // Remove In HUD
                if(activatable && owner.TryGetComponent(out Character character) && character.isPlayer)
                {
                    character.inventoryEvents.OnActivatableDrop?.Invoke(this);
                }

                // Inventory Transfer to Dropped Object
                if (droppedObject.GetComponent<InventoryPickup>() is var io && io)
                {
                    io.inventory = this;
                    io.countDownDestroy = droppedLifeTime > 0;
                    io.lifeTime = droppedLifeTime;
                }

                { // Set velocity
                    Rigidbody newRB = droppedObject.GetComponent<Rigidbody>();
                    Rigidbody ownRB = owner.GetComponent<Rigidbody>();
                    IGravityUser newGU = droppedObject.GetComponent<IGravityUser>();
                    IGravityUser ownGU = owner.GetComponent<IGravityUser>();
                    Vector3 ownVel = ownGU != null ? ownGU.Velocity : ownRB != null ? ownRB.velocity : Vector3.zero;

                    if (newRB)
                        newRB.velocity += ownVel;
                    else if (newGU != null)
                        newGU.Velocity += ownVel;
                }

                // Clipping prevention
                if(droppedObject.TryGetComponent(out Collider collider))
                    droppedObject.transform.position += collider.transform.position - collider.ClosestPoint(collider.transform.position - dropPoint.normal * 100);
            }
            else
                droppedObject = null;

            return destroyOnDrop || droppedObject;
        }

        private bool OnPickupInternal(InventoryContainer container)
        {
            bool valid = OnPickup(container.gameObject);

            // Activatables
            if (activatable && active)
                OnActivate(container.gameObject);

            // Display Pickup Message
            if (container.TryGetComponent(out Character character) && character.isPlayer)
            {
                if (character.pickupMessager && valid)
                    character.pickupMessager.Invoke(new MessageEventParameters() { message = $"Acquired {displayName}" });

                if (activatable)
                    character.inventoryEvents.OnActivatablePickup?.Invoke(this);
            }

            return valid;
        }

        /// <summary> Child-defined code for approving pick-ups </summary>
        /// <remarks> OnPickup is essentially the OnEnable of Inventory.
        /// It is called before the item is copied for pickup. </remarks>
        /// <param name="picker"> The active gameObject that is trying to pick this item up </param>
        /// <returns> true when the pick-up is approved </returns>
        public virtual bool OnPickup(GameObject picker)
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
        public virtual bool OnDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            return true;
        }

        public bool SetActive(GameObject owner, bool active)
        {
            if (this.active != active)
                if (active)
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
    }
}
 