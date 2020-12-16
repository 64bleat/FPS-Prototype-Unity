using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGUITableItem
{
    GUITableItem GetTableInfo();
}

public struct GUITableItem
{
    public Dictionary<string, string> tableValues;
    public Dictionary<string, Action<GameObject[]>> contextMenuItems;
}
