using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace MPGUI
{
    public class GUITable : MonoBehaviour, IGUIClickable
    {
        private enum SelectMode { Single, Add, Invert, Remove }

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

        private readonly Dictionary<GameObject, object> tableRowItems = new Dictionary<GameObject, object>();
        private readonly HashSet<GameObject> selection = new HashSet<GameObject>();

        [Serializable]
        public struct ColumnInfo
        {
            public string name;
            public float width;
        }

        public void GenerateTable(object[] items)
        {
            float totalWidth = 0;
            float totalHeight = 0;

            // Clear data
            tableRowItems.Clear();
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

            //Spawn new columns
            foreach (ColumnInfo c in columns)
            {
                GameObject go = Instantiate(columnTemplate, columnTemplate.transform.parent);
                TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
                RectTransform rt = go.transform as RectTransform;

                text.text = c.name;
                rt.offsetMax = new Vector2(rt.offsetMin.x + c.width, rt.offsetMax.y);
                totalWidth += rt.rect.width;
                go.SetActive(true);
            }

            //Required columns
            HashSet<string> columnHash = new HashSet<string>();
            foreach (ColumnInfo c in columns)
                columnHash.Add(c.name);

            //Spawn new items
            if (items != null)
                foreach (object item in items)
                {
                    GameObject go = Instantiate(itemTemplate, itemTemplate.transform.parent);
                    RectTransform rtrans = go.transform as RectTransform;
                    Dictionary<string, string> rowValues = new Dictionary<string, string>();

                    foreach (FieldInfo field in item.GetType().GetFields())
                        if (field.GetCustomAttribute(typeof(GUITableValueAttribute)) is GUITableValueAttribute attribute
                            && columnHash.Contains(attribute.column)
                            && !rowValues.ContainsKey(attribute.column))
                            rowValues.Add(attribute.column, field.GetValue(item) as string ?? "");

                    foreach (PropertyInfo property in item.GetType().GetProperties())
                        if (property.GetCustomAttribute(typeof(GUITableValueAttribute)) is GUITableValueAttribute attribute
                            && columnHash.Contains(attribute.column)
                            && !rowValues.ContainsKey(attribute.column))
                            rowValues.Add(attribute.column, property.GetValue(item) as string ?? "");

                    foreach (ColumnInfo c in columns)
                    {
                        GameObject f = Instantiate(itemFieldTemplate, go.transform);
                        TextMeshProUGUI text = f.GetComponentInChildren<TextMeshProUGUI>();
                        RectTransform rt = f.transform as RectTransform;

                        if (rowValues.TryGetValue(c.name, out string value))
                            text.SetText(value);
                        else
                            text.SetText("");

                        rt.offsetMax = new Vector2(rt.offsetMin.x + c.width, rt.offsetMax.y);
                        f.SetActive(true);
                    }

                    rtrans.offsetMax = new Vector2(rtrans.offsetMin.x + totalWidth, rtrans.offsetMax.y);
                    totalHeight += rtrans.rect.height;
                    tableRowItems.Add(go, item);
                    go.SetActive(true);
                }

            //set table width
            itemWindow.offsetMax = new Vector2(itemWindow.offsetMin.x + totalWidth, itemWindow.offsetMax.y);
            itemWindow.offsetMin = new Vector2(itemWindow.offsetMin.x, itemWindow.offsetMax.y - totalHeight);
        }

        private void Select(GameObject item, SelectMode mode)
        {
            if (tableRowItems.ContainsKey(item))
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
            {
                List<(string name, MethodInfo method, object instance)> contextMenuEntries = new List<(string, MethodInfo, object)>();

                Select(mouse.downInfo.gameObject, SelectMode.Add);

                // Get Context Menu Entries
                if (tableRowItems.TryGetValue(mouse.downInfo.gameObject, out object item))
                    foreach (MethodInfo method in item.GetType().GetMethods())
                        if (method.GetCustomAttribute(typeof(GUIContextMenuOptionAttribute)) is GUIContextMenuOptionAttribute attribute)
                            contextMenuEntries.Add((attribute.name, method, item));

                if (contextMenuEntries.Count > 0)
                {
                    GUIWindow window = GetComponentInParent<GUIWindow>();
                    GameObject contextMenu = Instantiate(contextMenuTemplate, mouse.downInfo.screenPosition, transform.rotation, window.transform.parent);
                    GUIButtonSet buttonSet = contextMenu.GetComponentInChildren<GUIButtonSet>();

                    foreach (var entry in contextMenuEntries)
                        buttonSet.AddButton(entry.name, () => entry.method.Invoke(entry.instance, null));
                }
            }
        }
        public void OnMouseHover(MouseInfo mouse) { }
        public void OnMousePress(MouseInfo mouse) { }
        public void OnMouseHold(MouseInfo mouse) { }
        public void OnMouseRelease(MouseInfo mouse) { }
    }
}
