using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Style info for <c>TablePanel</c>
    /// </summary>
    public class TableStyle : ScriptableObject
    {
        [SerializeField] internal Color backgroundColor = Color.clear;
        [SerializeField] internal Color textColor = Color.black;
        [SerializeField] internal Color selectedColor = Color.blue;
        [SerializeField] internal Color selectedTextColor = Color.white;
        [SerializeField] internal GameObject rowTemplate;
        [SerializeField] internal GameObject columnTemplate;
        [SerializeField] internal GameObject fieldTemplate;
        [SerializeField] internal GameObject contextMenuTemplate;
    }
}
