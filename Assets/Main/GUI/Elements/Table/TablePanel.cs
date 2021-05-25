using MPCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    ///     Displays a list of dynamic objects in the form of a table
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only Fields and Properties tagged with the <c>Tabulate</c> attribute 
    ///         with a matching column name will appear.
    ///     </para>
    ///     <para>
    ///         Actions tagged with the <c>ContextMenu</c> attribute will appear
    ///         in the context menu, as well as any method addedto <c>universalMethods</c>
    ///     </para>
    /// </remarks>
    public class TablePanel : MonoBehaviour, IClickable
    {
        public readonly List<ContextMethod> universalMethods = new List<ContextMethod>();

        [SerializeField] private TableStyle style;
        [SerializeField] private RectTransform tablePanel;
        [SerializeField] private RectTransform rowsPanel;
        [SerializeField] private RectTransform legendPanel;
        [SerializeField] private ColumnInfo[] columns = null;

        public struct ContextMethod
        {
            public string name;
            public Action<dynamic> action;
            public Type type;
        }

        private readonly Dictionary<GameObject, object> rows = new Dictionary<GameObject, object>();
        private readonly HashSet<GameObject> selection = new HashSet<GameObject>();

        [Serializable]
        public struct ColumnInfo
        {
            public string name;
            public float width;
        }

        public void GenerateTable(dynamic[] entries)
        {
            float tableWidth = 0;
            float tableHeight = 0;

            // Clear Data
            rows.Clear();
            selection.Clear();

            // Clear Panel
            for (int i = 0, count = legendPanel.childCount; i < count; i++)
                Destroy(legendPanel.GetChild(i).gameObject);

            for (int i = 0, count = rowsPanel.childCount; i < count; i++)
                Destroy(rowsPanel.GetChild(i).gameObject);

            // Generate Legend
            foreach (ColumnInfo info in columns)
            {
                GameObject column = Instantiate(style.columnTemplate, legendPanel);
                RectTransform columnRect = column.transform as RectTransform;
                Vector2 size = columnRect.sizeDelta;

                if(column.TryGetComponentInChildren(out TextMeshProUGUI text))
                    text.SetText(info.name);

                size.x = info.width;
                columnRect.sizeDelta = size;
                tableWidth += columnRect.rect.width;
                column.SetActive(true);
            }

            // Generate Schema
            HashSet<string> schema = new HashSet<string>();
            foreach (ColumnInfo c in columns)
                schema.Add(c.name);

            // Generate Rows
            if (entries != null)
            {
                Dictionary<string, string> rowValues = new Dictionary<string, string>();
                Type tabType = typeof(TabulateAttribute);

                foreach (object entry in entries)
                {
                    GameObject row = Instantiate(style.rowTemplate, rowsPanel);
                    RectTransform rowRect = row.transform as RectTransform;
                    Vector2 rowSize = rowRect.sizeDelta;
                    Type entryType = entry.GetType();

                    // Search Fields
                    foreach (FieldInfo field in entry.GetType().GetFields())
                        if (field.GetCustomAttribute(tabType) is TabulateAttribute attribute
                            && schema.Contains(attribute.columnName)
                            && !rowValues.ContainsKey(attribute.columnName))
                            rowValues.Add(attribute.columnName, field.GetValue(entry)?.ToString() ?? "ERROR");

                    // Search Properties
                    foreach (PropertyInfo property in entry.GetType().GetProperties())
                        if (property.GetCustomAttribute(tabType) is TabulateAttribute attribute
                            && schema.Contains(attribute.columnName)
                            && !rowValues.ContainsKey(attribute.columnName))
                            rowValues.Add(attribute.columnName, property.GetValue(entry)?.ToString() ?? "ERROR");

                    // Generate Row Fields
                    foreach (ColumnInfo column in columns)
                    {
                        GameObject field = Instantiate(style.fieldTemplate, row.transform);
                        RectTransform rt = field.transform as RectTransform;
                        Vector2 size = rt.sizeDelta;

                        if(field.TryGetComponentInChildren(out TextMeshProUGUI text))
                            if (rowValues.TryGetValue(column.name, out string value))
                                text.SetText(value);
                            else
                                text.SetText("ERR");

                        size.x = column.width;
                        rt.sizeDelta = size;
 
                        field.SetActive(true);
                    }

                    // Add Row
                    rowSize.x = tableWidth;
                    rowRect.sizeDelta = rowSize;
                    tableHeight += rowSize.y;
                    rows.Add(row, entry);
                    row.SetActive(true);
                    rowValues.Clear();
                }
            }

            //set table width
            rowsPanel.sizeDelta = new Vector2(tableWidth, tableHeight);
            tablePanel.sizeDelta = new Vector2(tableWidth, tableHeight + legendPanel.sizeDelta.y);
        }

        private void SetSelection(GameObject item, bool selected)
        {
            if (item && (selected ^ selection.Contains(item)))
            {
                foreach (TextMeshProUGUI text in item.GetComponentsInChildren<TextMeshProUGUI>())
                    text.color = selected ? style.selectedTextColor : style.textColor;

                foreach (UnityEngine.UI.Image image in item.GetComponents<UnityEngine.UI.Image>())
                    image.color = selected ? style.selectedColor : style.backgroundColor;

                if (selected)
                    selection.Add(item);
                else
                    selection.Remove(item);
            }
        }

        public void OnMouseClick(MouseInfo mouse)
        {
            GameObject item = mouse.downInfo.gameObject;

            if (rows.ContainsKey(item))
                if (mouse.Contains(KeyCode.Mouse0))
                {
                    // Add
                    if (mouse.Contains(KeyCode.LeftShift) || mouse.Contains(KeyCode.RightShift))
                        SetSelection(item, true);
                    // Invert
                    else if (mouse.Contains(KeyCode.LeftControl) || mouse.Contains(KeyCode.RightControl))
                        SetSelection(item, !selection.Contains(item));
                    // Remove
                    else if (mouse.Contains(KeyCode.LeftAlt) || mouse.Contains(KeyCode.RightAlt))
                        SetSelection(item, false);
                    // Single
                    else
                    {
                        bool select = selection.Count > 1 || !selection.Contains(item);

                        foreach (GameObject go in selection.ToArray())
                            SetSelection(go, false);

                        SetSelection(item, select);
                    }
                }
                else if (mouse.Contains(KeyCode.Mouse1))
                {
                    SetSelection(item, true);
                    SpawnContextMenu(item, mouse.downInfo.screenPosition);
                }
        }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }

        private void SpawnContextMenu(GameObject clicked, Vector2 screenPosition)
        {
            List<(string name, MethodInfo method, object instance)> contextMenuEntries = new List<(string, MethodInfo, object)>();

            if (rows.TryGetValue(clicked, out object item))
            {
                Window window = GetComponentInParent<Window>();
                GameObject contextMenu = Instantiate(style.contextMenuTemplate, screenPosition, transform.rotation, window.transform.parent);
                ButtonSet buttonSet = contextMenu.GetComponentInChildren<ButtonSet>();
                Type itemType = item.GetType();

                // Universal Entries
                foreach (var entry in universalMethods)
                    if (entry.type == itemType || itemType.IsSubclassOf(entry.type))
                        buttonSet.AddButton(entry.name, () => entry.action.Invoke(item));

                // Class-Specific Entries
                foreach (MethodInfo method in item.GetType().GetMethods())
                    if (method.GetCustomAttribute(typeof(ContextMenuAttribute)) is ContextMenuAttribute attribute)
                        buttonSet.AddButton(attribute.name, () => method.Invoke(item, null));
            }
        }
    }
}
