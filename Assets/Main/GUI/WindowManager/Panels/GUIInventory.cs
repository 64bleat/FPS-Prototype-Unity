using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Load a table with inventory data
    /// </summary>
    public class GUIInventory : MonoBehaviour
    {
        private Character owner;
        private GUITable table;

        private void OnEnable()
        {
            table = GetComponent<GUITable>();
            owner = CameraManager.target.GetComponentInParent<Character>();

            table.universalMethods.Clear();
            table.universalMethods.Add(new GUITable.ContextMethod()
            {
                type = typeof(Inventory),
                action = CharacterDrop,
                name = "Drop"
            });

            table.GenerateTable(owner.inventory.ToArray());
        }

        private void CharacterDrop(dynamic o)
        {
            if (o is Inventory i)
                InventoryManager.Drop(
                    owner.inventory,
                    owner.inventory.IndexOf(i),
                    owner.transform.TransformPoint(Vector3.forward * 2),
                    owner.transform.rotation,
                    owner.gameObject,
                    default);

            table.GenerateTable(owner.inventory.ToArray());
        }
    }
}