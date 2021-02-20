using MPConsole;
using MPWorld;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class InventoryContainer : MonoBehaviour
    {
        public List<Inventory> inventory;
        public MessageEvent playerPickupMessageEvent;
        public PlayerInventoryEvents playerInventoryEvents;

        private Character character;

        private void Awake()
        {
            TryGetComponent(out character);

            // Don't store direct references to Inventory resources 
            for (int i = 0; i < inventory.Count; i++)
                if (!inventory[i].staticReference)
                    inventory[i] = Instantiate(inventory[i]);

            Console.RegisterInstance(this);
        }

        private void OnDestroy()
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
            foreach (Inventory item in inventory)
                if(item.asset == reference.asset)
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
                inventory.Add(instance);

                return true;
            }
            else if (instance != this)
                Destroy(instance);

            return false;
        }

        private bool Pickup(Inventory instance)
        {
            bool valid = instance.OnPickup(gameObject);

            // Activatables
            if (instance.activatable)
            {
                instance.active = false;
                instance.SetActive(gameObject, true);
            }

            // Display Pickup Message
            if (character ? character.isPlayer : false)
            {
                if (playerPickupMessageEvent && valid)
                    playerPickupMessageEvent.Invoke(new MessageEventParameters() { message = $"Acquired {instance.displayName}" });

                if (instance.activatable)
                    playerInventoryEvents.OnActivatablePickup?.Invoke(instance);
            }

            return valid;
        }

        /// <summary> Players pick up an item using the console </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        [ConsoleCommand("pickup")]
        public string ConsolePickup(string resourcePath)
        {
            if (character ? character.isPlayer : false)
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

        public bool TryDrop(Inventory item, Vector3 position, Quaternion rotation, RaycastHit dropPoint, out GameObject droppedObject)
        {
            if (item.OnDrop(gameObject, position, rotation) 
                && !item.destroyOnDrop 
                && item.sceneObjectPrefab)
            {
                // Deactivate if Active
                item.SetActive(gameObject, false);

                droppedObject = Instantiate(item.sceneObjectPrefab, position, rotation);

                // Remove In HUD
                if (item.activatable && (character ? character.isPlayer : false))
                    playerInventoryEvents.OnActivatableDrop?.Invoke(item);

                // Transfer to Pickup
                if(droppedObject.TryGetComponent(out InventoryPickup pickup))
                {
                    pickup.inventory = item;
                    pickup.countDownDestroy = item.droppedLifeTime > 0;
                    pickup.lifeTime = item.droppedLifeTime;
                }

                // Set velocity
                Vector3 ownVel;

                if (TryGetComponent(out IGravityUser ownGU))
                    ownVel = ownGU.Velocity;
                else if (TryGetComponent(out Rigidbody ownRB))
                    ownVel = ownRB.velocity;
                else
                    ownVel = Vector3.zero;

                if (droppedObject.TryGetComponent(out Rigidbody newRB))
                    newRB.velocity += ownVel;
                else if (droppedObject.TryGetComponent(out IGravityUser newGU))
                    newGU.Velocity += ownVel;

                // Clipping prevention
                if (droppedObject.TryGetComponent(out Collider collider))
                    droppedObject.transform.position += collider.transform.position - collider.ClosestPoint(collider.transform.position - dropPoint.normal * 100);
            }
            else
                droppedObject = null;

            return item.destroyOnDrop || droppedObject;
        }

        public GameObject Drop(int index, Vector3 position, Quaternion rotation, RaycastHit hit)
        {
            if(TryDrop(inventory[index], position, rotation, hit, out GameObject droppedObject))
            {
                inventory.RemoveAt(index);

                if (droppedObject.TryGetComponent(out InventoryPickup pickup))
                    pickup.OnDropped(gameObject);
            }

            return droppedObject;
        }

        /// <summary> Find an instance of an item within this container </summary>
        /// <param name="reference"></param>
        /// <param name="instance"></param>
        /// <returns> true if a clone was found </returns>
        public bool TryFind(Inventory reference, out Inventory instance)
        {
            try
            {
                instance = inventory.Find((i) => i.asset == reference.asset);
                return true;
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
                    inventory.Remove(item);

        }
    }
}
