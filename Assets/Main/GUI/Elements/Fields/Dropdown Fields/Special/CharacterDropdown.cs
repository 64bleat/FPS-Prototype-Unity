using MPCore;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class CharacterDropdown : DropdownField
    {
        private PlayerSettingsModel _settingsModel;

        private void Awake()
        {
            _settingsModel = Models.GetModel<PlayerSettingsModel>();
        }

        protected override void InitField()
        {
            if(_settingsModel.characterInfo)
            valueText.SetText(_settingsModel.characterInfo.bodyType.gameObject.name);
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            foreach (MPCore.Character c in _settingsModel.availableCharacters)
            {
                MPCore.Character character = c;
                void Call()
                {
                    _settingsModel.characterInfo.bodyType = character;
                    InitField();
                }
                GameObject go = set.AddButton(character.gameObject.name, Call);

                if (go.TryGetComponent(out Image image) && character != _settingsModel.characterInfo.bodyType)
                    image.color *= 0.5f;
            }
        }
    }
}
