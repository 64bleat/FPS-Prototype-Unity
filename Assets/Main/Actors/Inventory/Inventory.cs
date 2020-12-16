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
        
        public string displayName;

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

        [GUITableValue("Count")]
        public string Count => maxCount > 1 ? $"{count}/{maxCount}" : "";
        [GUITableValue("Name")]
        public string Name => displayName;

        [GUIContextMenuOption("Test Void")]
        public void InventoryTestMethod()
        {
            Debug.Log($"Testing {name}");
        }

        /// <summary> Pickup the item and call OnPickup, returns true if pickup was a success. </summary>
        public bool TryPickup(Character character)
        {
            if (character && character.inventory != null)
            {
                // Destroy on Pickup
                if (destroyOnPickup)
                    return OnPickup(character.gameObject);

                // Modify existing inventory item
                foreach (Inventory item in character.inventory)
                    if (item.displayName.Equals(displayName))
                        if (item.count >= item.maxCount)
                            return false;
                        else if (OnPickup(character.gameObject))
                        {
                            item.count = Mathf.Min(item.maxCount, item.count + count);
                            return true;
                        }
                        else
                            return false;

                // Add new item to inventory
                Inventory instance = staticReference ? this : Instantiate(this);

                if (instance.OnPickup(character.gameObject))
                {
                    character.inventory.Add(instance);

                    return true;
                }
                else if (!staticReference)
                    Destroy(instance);
            }

            return false;
        }

        /// <summary> Drop the item and call OnDrop. returns true if drop was a success. </summary>
        public bool TryDrop(GameObject owner, Vector3 position, Quaternion rotation, RaycastHit dropPoint, out GameObject droppedObject)
        {
            droppedObject = null;

            if (OnDrop(owner, position, rotation) && !destroyOnDrop && sceneObjectPrefab)
            {
                droppedObject = Instantiate(sceneObjectPrefab, position, rotation);

                // Inventory Transfer to Dropped Object
                if (droppedObject.GetComponent<InventoryObject>() is var io && io)
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
                if (dropPoint.collider
                    && droppedObject.GetComponent<Collider>() is var collider && collider)
                    droppedObject.transform.position += collider.transform.position - collider.ClosestPoint(collider.transform.position - dropPoint.normal * 100);
            }

            return destroyOnDrop || droppedObject;
        }

        /// <summary> Child-defined code for picking up an item </summary>
        /// <param name="owner"> The active gameObject that is trying to pick this item up </param>
        /// <returns> true when the item is ready to be picked up </returns>
        public virtual bool OnPickup(GameObject owner)
        {
            return true;
        }

        /// <summary> Child-defined code for dropping an item </summary>
        /// <param name="owner"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public virtual bool OnDrop(GameObject owner, Vector3 position, Quaternion rotation)
        {
            return true;
        }
    }
}
