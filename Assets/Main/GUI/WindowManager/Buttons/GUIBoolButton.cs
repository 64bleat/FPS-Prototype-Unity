using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GUIBoolButton : MonoBehaviour
{
    public bool value;
    public TextMeshProUGUI description;
    public TextMeshProUGUI valueName;
    public UnityEvent<bool> OnValueChange;

    private void OnValidate()
    {
        valueName.SetText(value.ToString());
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
