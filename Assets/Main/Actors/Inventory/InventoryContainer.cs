using MPConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class InventoryContainer : MonoBehaviour
    {
        public List<Inventory> inventory;

        private void Awake()
        {
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

        public GameObject Drop(int index, Vector3 position, Quaternion rotation, RaycastHit hit)
        {
            if (inventory[index].TryDrop(gameObject, position, rotation, hit, out GameObject droppedObject))
            {
                inventory.RemoveAt(index);

                if (droppedObject.TryGetComponent(out InventoryPickup pickup))
                    pickup.OnDropped(gameObject);
            }

            return droppedObject;
        }

        public bool TryFind(Inventory reference, out Inventory item)
        {
            try
            {
                item = inventory.Find((i) => i.displayName == reference.displayName);
                return true;
            }
            catch
            {
                item = null;
                return false;
            }
        }

        public void Remove(Inventory reference, int count = 1)
        {
            if (TryFind(reference, out Inventory item))
                if ((item.count -= count) <= 0)
                    inventory.Remove(item);

        }

        [ConsoleCommand("pickup")]
        public string ConsolePickup(string path)
        {
            if (TryGetComponent(out Character character) && character.isPlayer)
            {
                Inventory resource = Resources.Load<Inventory>(path);

                if (resource)
                    //if (InventoryManager.PickUp(this, resource))
                    if(resource.TryPickup(this, out _))
                        return $"Picked up {resource.name}";
                    else
                        return $"Could not pick up {resource.name}";
                else
                    return $"Could not find resource {path}";
            }
            else
                return null;
        }
    }
}
