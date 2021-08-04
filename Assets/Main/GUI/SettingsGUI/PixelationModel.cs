using MPCore;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class PixelationModel : Models
    {
        public Material[] shaders;
        public Material currentShader;
        [SerializeField] private Dropdown dropdown;

        public void DropdownShader(RectTransform button)
        {
            ButtonSet set = dropdown.SpawnDropdown(button);

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
