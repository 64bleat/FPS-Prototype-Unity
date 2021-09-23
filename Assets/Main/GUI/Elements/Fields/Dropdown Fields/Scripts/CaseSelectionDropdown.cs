using MPCore;
using System.Linq;

namespace MPGUI
{
	public sealed class CaseSelectionDropdown : GenericDropdownField<Case>
	{
		//void Awake()
		//{
		//	PlaySettingsModel playmodel = Models.GetModel<PlaySettingsModel>();

		//	SetReference(playmodel, nameof(playmodel.scene));
		//	AddOptions(ResourceLoader.GetResources<Case>().OrderBy(c => Write(c)));
		//}

		protected override string Write(Case value)
		{
			return value ? value.displayName : "None";
		}
	}
}
