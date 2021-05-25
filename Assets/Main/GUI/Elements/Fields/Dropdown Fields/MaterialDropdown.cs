using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MPGUI
{
    public class MaterialDropdown : DropdownField
    {
        [SerializeField] private Object instance;
        [SerializeField] private string fieldName;
        public List<Material> options;

        protected override void InitField()
        {
            Material mat = GetField().GetValue(instance) as Material;
            valueText.SetText(mat ? mat.name : "Off");
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            foreach (Material opt in options)
            {
                Material mat = opt;
                string text = mat ? mat.name : "Off";
                void call()
                {
                    GetField().SetValue(instance, mat);
                    InitField();
                }
                Image image = set.AddButton(text, call)
                    .GetComponent<Image>();

                if (image && text != valueText.text)
                    image.color *= 0.5f;
            }
        }

        private FieldInfo GetField()
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldName);

            return field;
        }    
    }
}
