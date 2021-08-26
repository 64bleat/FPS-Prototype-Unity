using MPCore;

namespace MPGUI
{
    public class GameDropdown : GenericDropdownField<GameController>
    {
        void Awake()
        {
            PlaySettingsModel _selection = Models.GetModel<PlaySettingsModel>();
            SetReference(_selection, nameof(_selection.game));
            AddOptions(ResourceLoader.GetResources<GameController>());
        }

        protected override string Write(GameController value)
        {
            return value.gameObject.name;
        }
    }
}
