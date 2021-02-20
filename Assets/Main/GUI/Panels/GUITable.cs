using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using MPCore;

namespace MPGUI
{
    public class GUITable : MonoBehaviour, IGUIClickable
    {
        private enum SelectMode { Single, Add, Invert, Remove }

        public readonly List<ContextMethod> universalMethods = new List<ContextMethod>();

        [SerializeField] private Color defaultBackgroundColor = new Color(1, 1, 1, 0);
        [SerializeField] private Color defaultTextColor = new Color(0, 0, 0, 1);
        [SerializeField] private Color selectBackgroundColor = new Color(0, 0, 0.5f, 1);
        [SerializeField] private Color selectTextColor = new Color(1, 1, 1, 1);
        [SerializeField] private RectTransform itemWindow = null;
        [SerializeField] private GameObject itemTemplate = null;
        [SerializeField] private GameObject itemFieldTemplate = null;
        [SerializeField] private GameObject columnTemplate = null;
        [SerializeField] private GameObject contextMenuTemplate = null;
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

            // Clear data
            rows.Clear();
            selection.Clear();

            //remove old columns
            foreach (Transform t in columnTemplate.transform.parent)
                if (!columnTemplate.transform.Equals(t) && !columnTemplate.transform.parent.Equals(t))
                    Destroy(t.gameObject);

            //remove old items
            foreach (Transform t in itemTemplate.transform.parent)
                if (!itemTemplate.transform.Equals(t) && !itemTemplate.transform.parent.Equals(t))
                    Destroy(t.gameObject);

            //hideTemplate
            itemTemplate.SetActive(false);
            itemFieldTemplate.SetActive(false);
            columnTemplate.SetActive(false);

            //Table Columns
            foreach (ColumnInfo info in columns)
            {
                GameObject column = Instantiate(columnTemplate, columnTemplate.transform.parent);
                RectTransform columnRect = column.transform as RectTransform;
                TextMeshProUGUI text = column.GetComponentInChildren<TextMeshProUGUI>();

                text.SetText(info.name);
                columnRect.offsetMax = new Vector2(columnRect.offsetMin.x + info.width, columnRect.offsetMax.y);
                tableWidth += columnRect.rect.width;
                column.SetActive(true);
            }

            //Table Schema
            HashSet<string> schema = new HashSet<string>();
            foreach (ColumnInfo c in columns)
                schema.Add(c.name);

            //Table Rows
            if (entries != null)
            {
                Dictionary<string, string> rowValues = new Dictionary<string, string>();
                Type tableValue = typeof(GUITableValueAttribute);

                foreach (dynamic entry in entries)
                {
                    GameObject row = Instantiate(itemTemplate, itemTemplate.transform.parent);
                    RectTransform rowRect = row.transform as RectTransform;

                    foreach (FieldInfo field in entry.GetType().GetFields())
                        if (field.GetCustomAttribute(tableValue) is GUITableValueAttribute attribute
                            && schema.Contains(attribute.columnName)
                            && !rowValues.ContainsKey(attribute.columnName))
                            rowValues.Add(attribute.columnName, field.GetValue(entry)?.ToString() ?? "ERROR");

                    foreach (PropertyInfo property in entry.GetType().GetProperties())
                        if (property.GetCustomAttribute(tableValue) is GUITableValueAttribute attribute
                            && schema.Contains(attribute.columnName)
                            && !rowValues.ContainsKey(attribute.columnName))
                            rowValues.Add(attribute.columnName, property.GetValue(entry)?.ToString() ?? "ERROR");

                    foreach (ColumnInfo column in columns)
                    {
                        GameObject f = Instantiate(itemFieldTemplate, row.transform);
                        TextMeshProUGUI text = f.GetComponentInChildren<TextMeshProUGUI>();
                        RectTransform rt = f.transform as RectTransform;

                        if (rowValues.TryGetValue(column.name, out string value))
                            text.SetText(value);
                        else
                            text.SetText("XXX");

                        rt.offsetMax = new Vector2(rt.offsetMin.x + column.width, rt.offsetMax.y);
                        f.SetActive(true);
                    }

                    rowRect.offsetMax = new Vector2(rowRect.offsetMin.x + tableWidth, rowRect.offsetMax.y);
                    tableHeight += rowRect.rect.height;
                    rows.Add(row, entry);
                    row.SetActive(true);
                    rowValues.Clear();
                }
            }

            //set table width
            itemWindow.offsetMax = new Vector2(itemWindow.offsetMin.x + tableWidth, itemWindow.offsetMax.y);
            itemWindow.offsetMin = new Vector2(itemWindow.offsetMin.x, itemWindow.offsetMax.y - tableHeight);
        }

        private void Select(GameObject item, SelectMode mode)
        {
            if (rows.ContainsKey(item))
                switch (mode)
                {
                    case SelectMode.Single:
                        bool select = selection.Count > 1 || !selection.Contains(item);

                        foreach (GameObject go in selection.ToArray())
                            SetSelection(go, false);

                        SetSelection(item, select);
                        break;
                    case SelectMode.Add:
                        SetSelection(item, true);
                        break;
                    case SelectMode.Invert:
                        SetSelection(item, !selection.Contains(item));
                        break;
                    case SelectMode.Remove:
                        SetSelection(item, false);
                        break;
                }
        }

        private void SetSelection(GameObject item, bool selected)
        {
            if (item && (selected ^ selection.Contains(item)))
            {
                foreach (TextMeshProUGUI text in item.GetComponentsInChildren<TextMeshProUGUI>())
                    text.color = selected ? selectTextColor : defaultTextColor;

                foreach (UnityEngine.UI.Image image in item.GetComponents<UnityEngine.UI.Image>())
                    image.color = selected ? selectBackgroundColor : defaultBackgroundColor;

                if (selected)
                    selection.Add(item);
                else
                    selection.Remove(item);
            }
        }

        public void OnMouseClick(MouseInfo mouse)
        {
            if (mouse.Contains(KeyCode.Mouse0))
            {
                if (mouse.Contains(KeyCode.LeftShift) || mouse.Contains(KeyCode.RightShift))
                    Select(mouse.downInfo.gameObject, SelectMode.Add);
                else if (mouse.Contains(KeyCode.LeftControl) || mouse.Contains(KeyCode.RightControl))
                    Select(mouse.downInfo.gameObject, SelectMode.Invert);
                else if (mouse.Contains(KeyCode.LeftAlt) || mouse.Contains(KeyCode.RightAlt))
                    Select(mouse.downInfo.gameObject, SelectMode.Remove);
                else
                    Select(mouse.downInfo.gameObject, SelectMode.Single);
            }
            else if (mouse.Contains(KeyCode.Mouse1))
                SpawnContextMenu(mouse.downInfo.gameObject, mouse.downInfo.screenPosition);
        }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }

        private void SpawnContextMenu(GameObject clicked, Vector2 screenPosition)
        {
            List<(string name, MethodInfo method, object instance)> contextMenuEntries = new List<(string, MethodInfo, object)>();

            Select(clicked, SelectMode.Add);

            if (rows.TryGetValue(clicked, out object item))
            {
                GUIWindow window = GetComponentInParent<GUIWindow>();
                GameObject contextMenu = Instantiate(contextMenuTemplate, screenPosition, transform.rotation, window.transform.parent);
                GUIButtonSet buttonSet = contextMenu.GetComponentInChildren<GUIButtonSet>();
                Type itemType = item.GetType();

                // Universal Entries
                foreach (var entry in universalMethods)
                    if (entry.type == itemType || itemType.IsSubclassOf(entry.type))
                        buttonSet.AddButton(entry.name, () => entry.action.Invoke(item));

                // Class-Specific Entries
                foreach (MethodInfo method in item.GetType().GetMethods())
                    if (method.GetCustomAttribute(typeof(GUIContextMenuOptionAttribute)) is GUIContextMenuOptionAttribute attribute)
                        buttonSet.AddButton(attribute.name, () => method.Invoke(item, null));
            }
        }
    }
}
