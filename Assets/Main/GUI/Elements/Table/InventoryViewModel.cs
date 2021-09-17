using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Load a table with inventory data
    /// </summary>
    public class InventoryViewModel : MonoBehaviour
    {
        GameModel _gameModel;
        Window _window;
        InventoryManager _items;
        TablePanel _table;

        void Awake()
        {
            _table = GetComponent<TablePanel>();
            _gameModel = Models.GetModel<GameModel>();
            _window = GetComponentInParent<Window>();
        }

        void OnEnable()
        {
            if (_gameModel.currentView.Value)
            { 
                _items = _gameModel.currentView.Value.GetComponentInParent<InventoryManager>();

                if (_items)
                {
                    _table.universalMethods.Clear();
                    _table.universalMethods.Add(new TablePanel.ContextMethod()
                    {
                        type = typeof(Inventory),
                        action = ContextDrop,
                        name = "Drop"
                    });

                    _table.GenerateTable(_items.inventory.ToArray());
                }
                else
                    _window.CloseWindow();
            }
            else
                _window.CloseWindow();
        }

        void ContextDrop(dynamic o)
        {
            if (o is Inventory item)
                _items.Drop(
                    _items.inventory.IndexOf(item),
                    _items.transform.TransformPoint(Vector3.forward * 2),
                    _items.transform.rotation,
                    default);

            _table.GenerateTable(_items.inventory.ToArray());
        }
    }
}
