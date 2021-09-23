using MPCore;

namespace MPGUI
{
	public class CanvasScalerDropdown : GenericDropdownField<float>
	{
		void Awake()
		{
			GraphicsModel graphicsModel = Models.GetModel<GraphicsModel>();
			SetReference(graphicsModel.canvasScale, nameof(graphicsModel.canvasScale.Value), "GUI Scale");
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
