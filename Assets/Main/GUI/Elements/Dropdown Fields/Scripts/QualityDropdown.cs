using MPCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class QualityDropdown : GenericDropdownField<int>
    {
        public int Current
        {
            get => QualitySettings.GetQualityLevel();
            set => QualitySettings.SetQualityLevel(value);
        }
        void Awake()
        {
            int count = QualitySettings.names.Length;

            SetReference(this, nameof(Current), "Quality Level");

            for (int i = count - 1; i >= 0; i--)
                AddOption(i);
        }

        protected override string Write(int value)
        {
            return QualitySettings.names[value];
        }
    }
}
