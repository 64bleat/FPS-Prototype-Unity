using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPCore;

namespace MPCore
{
    public static class InventoryManager
    {
        public static bool PickUp(Character character, Inventory item)
        {
            if (character && item && item.TryPickup(character))
                return true;
            else
                return false;
        }

        public static GameObject Drop(List<Inventory> list, int index, Vector3 position, Quaternion rotation, GameObject owner, RaycastHit hit)
        {
            GameObject droppedObject = null;

            if(index >= 0 && index < list.Count)
                if(list[index].TryDrop(owner, position, rotation, hit, out droppedObject))
                {
                    // REMOVAL MUST HAPPEN BEFORE DROP IS SPAWNED. OTHERWISE IT JUST DOUBLES THE COUNT
                    list.RemoveAt(index);

                    if (droppedObject.GetComponent<InventoryObject>() is var io && io)
                        io.OnDropped(owner);
                }

            return droppedObject;
        }

        public static Inventory Find(List<Inventory> list, Inventory item)
        {
            return list.Find((i) => i.displayName.Equals(item.displayName));
        }

        public static void Remove(List<Inventory> list, Inventory inventoryType, int count)
        {
            if(list != null && inventoryType != null && count > 0
                && list.Find((i) => i.displayName.Equals(inventoryType.displayName)) is var item && item)
            {
                item.count -= count;

                if (item.count <= 0)
                    list.Remove(item);
            }
        }
    }
}
