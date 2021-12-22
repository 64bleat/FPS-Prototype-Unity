using MPCore;
using MPGame;

namespace MPGUI
{
	public class GameDropdown : GenericDropdownField<GameManager>
	{
		void Awake()
		{
			PlaySettingsModel _selection = Models.GetModel<PlaySettingsModel>();
			SetReference(_selection, nameof(_selection.game));
			AddOptions(ResourceLoader.GetResources<GameManager>());
		}

		protected override string Write(GameManager value)
		{
			return value.gameObject.name;
		}
	}
}
