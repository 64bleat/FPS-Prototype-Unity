using MPCore;
using MPGame;

namespace MPGUI
{
	public sealed class CaseSelectionDropdown : GenericDropdownField<Case>
	{
		protected override string Write(Case value)
		{
			return value ? value.displayName : "None";
		}
	}
}
