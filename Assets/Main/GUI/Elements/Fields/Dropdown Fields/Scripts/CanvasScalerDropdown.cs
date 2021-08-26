using MPCore;
using UnityEngine.UI;

namespace MPGUI
{
    public class CanvasScalerDropdown : GenericDropdownField<float>
    {
        void Awake()
        {
            CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
            SetReference(scaler, nameof(scaler.scaleFactor), "GUI Scale");
            AddOption(1f);
            AddOption(1.5f);
            AddOption(2f);
            AddOption(2.5f);
            AddOption(3f);
            AddOption(3.5f);
            AddOption(4f);
        }
    }
}
