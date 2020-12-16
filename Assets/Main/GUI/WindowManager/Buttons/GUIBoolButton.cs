using System;
using TMPro;
using UnityEngine;

public class GUIBoolButton : MonoBehaviour
{
    public bool value;
    public TextMeshProUGUI description;
    public TextMeshProUGUI valueName;
    public Action<bool> OnValueChange;

    private void Awake()
    {
        SetValue(value);
    }

    public void SetValue(bool b)
    {
        valueName.SetText(b.ToString());

        if (b != value)
        {
            value = b;
            OnValueChange?.Invoke(b);
        }
    }

    public void ToggleValue()
    {
        SetValue(!value);
    }
}
