using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class PixelationGUI : ScriptableObject
    {
        public Material[] shaders;
        public Material currentShader;
        public DropdownSpawn dropdown;

        public void DropdownShader(RectTransform button)
        {
            GUIButtonSet set = dropdown.SpawnDropdown(button);

            foreach (Material mat in shaders)
            {
                Material setMat = mat;
                string text = mat ? mat.name : "None";
                Image image = set.AddButton(text, () => currentShader = setMat)
                    .GetComponent<Image>();

                if (image && setMat != currentShader)
                    image.color *= 0.5f;
            }
        }
    }
}
