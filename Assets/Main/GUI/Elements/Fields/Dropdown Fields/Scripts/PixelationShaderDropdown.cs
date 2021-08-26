using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class PixelationShaderDropdown : GenericDropdownField<Material>
    {
        private void Awake()
        {
            GraphicsModel graphics = Models.GetModel<GraphicsModel>();

            SetReference(graphics, nameof(graphics.pixelationShader));
            AddOptions(graphics.pixelationOptions);
        }

        protected override string Write(Material value)
        {
            return value ? value.name : "None";
        }
    }
}
