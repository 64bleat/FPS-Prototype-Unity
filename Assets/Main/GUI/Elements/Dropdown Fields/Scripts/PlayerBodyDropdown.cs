using MPCore;

namespace MPGUI
{
	public sealed class PlayerBodyDropdown : GenericDropdownField<Character>
	{
		void Awake()
		{
			PlaySettingsModel _selection = Models.GetModel<PlaySettingsModel>();
			SetReference(_selection.playerProfile, nameof(_selection.playerProfile.bodyType));
			AddOptions(ResourceLoader.GetResources<Character>());
		}

		protected override string Write(Character value)
		{
			return value ? value.displayName : "None";
		}
	}
}
