using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MPCore
{
    [RequireComponent(typeof(Rigidbody))]
    public class PortaSpawn : InventoryPickup
    {
        public static Stack<GameObject> stack = new Stack<GameObject>();

        public Transform spawnPoint;

        private readonly List<Inventory> savedStuff = new List<Inventory>();

        private void OnDestroy()
        {
            stack = new Stack<GameObject>(from go in stack.ToArray()
                                          where !go.Equals(spawnPoint.gameObject)
                                          select go);
        }

        public void TransferStuff(InventoryManager container)
        {
            foreach (Inventory i in savedStuff)
                container.TryPickup(i, out _);
                //i.TryPickup(container, out _);
        }

        public override void OnDropped(GameObject dropper)
        {
            savedStuff.Clear();

            if (dropper)
            {
                if (dropper.TryGetComponent(out InventoryManager container))
                    foreach (Inventory i in container.inventory)
                        savedStuff.Add(Instantiate(i));

                stack.Push(spawnPoint.gameObject);
            }
        }
    }
}