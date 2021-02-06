using UnityEngine;

namespace MPGUI
{
    public class DropdownSpawn : ScriptableObject
    {
        public GameObject dropdownPrefab;

        public GUIButtonSet SpawnDropdown(RectTransform button)
        {
            Vector3 position = button.TransformPoint(new Vector3(button.rect.xMin, button.rect.yMin, 0));
            GameObject go = Instantiate(dropdownPrefab, position, button.rotation, button.parent);
            GUIScrollPanel p = go.GetComponentInChildren<GUIScrollPanel>();
            RectTransform r = p.transform as RectTransform;

            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, button.rect.width);
            r.SetSiblingIndex(button.GetSiblingIndex() + 1);

            return go.GetComponentInChildren<GUIButtonSet>();
        }
    }
}
