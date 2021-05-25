using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class QualityDropdown : DropdownField
    {
        [SerializeField] private ObjectEvent qualityChannel;

        protected override void InitField()
        {
            string[] qlNames = QualitySettings.names;
            int qlIndex = QualitySettings.GetQualityLevel();

            valueText.SetText(qlNames[qlIndex]);
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);
            string[] names = QualitySettings.names;
            int len = names.Length;
            Color good = new Color(0.5f, 1f, 0.5f);
            Color bad = new Color(0.8f, 0.5f, 0.2f);

            for (int i = len - 1; i >= 0; i--)
            {
                int l = i;
                void call(int level)
                {
                    QualitySettings.SetQualityLevel(level);
                    InitField();
                    qualityChannel.Invoke(level);
                }
                GameObject go = set.AddButton(names[l], () => call(l));
                Image image = go.GetComponent<Image>();

                if (image)
                    if (i == QualitySettings.GetQualityLevel())
                        image.color = Color.Lerp(bad, good, (float)i / len);
                    else
                        image.color *= 0.5f;
            }
        }


    }
}
