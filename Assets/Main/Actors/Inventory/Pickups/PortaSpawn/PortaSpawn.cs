using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MPCore
{
    [RequireComponent(typeof(Rigidbody))]
    public class PortaSpawn : InventoryObject
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

        public void TransferStuff(Character character)
        {
            foreach (Inventory i in savedStuff)
                i.TryPickup(character, out _);
        }

        public override void OnDropped(GameObject dropper)
        {
            savedStuff.Clear();

            if (dropper)
            {
                Character c = dropper.GetComponent<Character>();

                if (c)
                    foreach (Inventory i in c./*info.*/inventory)
                        savedStuff.Add(Instantiate(i));

                stack.Push(spawnPoint.gameObject);
            }
        }
    }
}