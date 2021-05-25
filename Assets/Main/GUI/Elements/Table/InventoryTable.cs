using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Load a table with inventory data
    /// </summary>
    public class InventoryTable : MonoBehaviour
    {
        private InventoryContainer container;
        private TablePanel table;

        private void OnEnable()
        {
            if (CameraManager.target)
            {
                table = GetComponent<TablePanel>();
                container = CameraManager.target.GetComponentInParent<InventoryContainer>();

                table.universalMethods.Clear();
                table.universalMethods.Add(new TablePanel.ContextMethod()
                {
                    type = typeof(Inventory),
                    action = ContextDrop,
                    name = "Drop"
                });

                table.GenerateTable(container.inventory.ToArray());
            }
            else if(gameObject.TryGetComponentInParent(out Window window))
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
