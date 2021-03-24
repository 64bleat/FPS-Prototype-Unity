using MPCore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MPGUI
{
    public abstract class ValueButton : MonoBehaviour
    {
        private protected void SetValueText(string text)
        {
            if (transform.TryFindChild("Value", out TextMeshProUGUI valueText))
                valueText.SetText(text);
            else
                Debug.LogWarning($"Button on {gameObject.name} needs text for Value", gameObject);
        }

        public void SetLabel(string label)
        {
            if (transform.TryFindChild("Label", out TextMeshProUGUI labelText))
                labelText.SetText(label);
        }
    }
}
