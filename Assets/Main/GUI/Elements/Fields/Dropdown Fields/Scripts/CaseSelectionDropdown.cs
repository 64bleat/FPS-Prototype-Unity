using MPCore;
using System.Linq;

namespace MPGUI
{
    public sealed class CaseSelectionDropdown : GenericDropdownField<Case>
    {
        void Awake()
        {
            PlaySettingsModel _selection = Models.GetModel<PlaySettingsModel>();
            SetReference(_selection, nameof(_selection.scene));
            AddOptions(ResourceLoader.GetResources<Case>().OrderBy(c => Write(c)));
        }

        protected override string Write(Case value)
        {
            return value.displayName;
        }
    }
}
