using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Load a table with inventory data
    /// </summary>
    public class GUIInventory : MonoBehaviour
    {
        private InventoryContainer container;
        private GUITable table;

        private void OnEnable()
        {
            if (CameraManager.target)
            {
                table = GetComponent<GUITable>();
                container = CameraManager.target.GetComponentInParent<InventoryContainer>();

                table.universalMethods.Clear();
                table.universalMethods.Add(new GUITable.ContextMethod()
                {
                    type = typeof(Inventory),
                    action = ContextDrop,
                    name = "Drop"
                });

                table.GenerateTable(container.inventory.ToArray());
            }
            else if(gameObject.TryGetComponentInParent(out GUIWindow window))
                window.gameObject.SetActive(false);
        }

        private void ContextDrop(dynamic o)
        {
            if (o is Inventory item)
                container.Drop(
                    container.inventory.IndexOf(item),
                    container.transform.TransformPoint(Vector3.forward * 2),
                    container.transform.rotation,
                    default);

            table.GenerateTable(container.inventory.ToArray());
        }
    }
}
