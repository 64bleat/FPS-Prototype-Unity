using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class CanvasScalerDropdown : DropdownField
    {
        protected override void InitField()
        {
            if (gameObject.TryGetComponentInParent(out CanvasScaler cs))
                valueText.SetText(cs.scaleFactor.ToString());
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);
            CanvasScaler scaler = gameObject.GetComponentInParent<CanvasScaler>();

            for (int i = 1; i <= 4; i++)
            {
                int factor = i;
                void Call()
                {
                    scaler.scaleFactor = factor;
                    InitField();
                }
                Image image = set.AddButton(factor.ToString(), Call).GetComponent<Image>();

                if (image && factor != scaler.scaleFactor)
                    image.color *= 0.5f;
            }
        }
    }
}
