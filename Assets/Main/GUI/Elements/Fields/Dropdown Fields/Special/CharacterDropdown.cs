using MPCore;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class CharacterDropdown : DropdownField
    {
        [SerializeField] private PlayerSettingsManager profile;

        protected override void InitField()
        {
            valueText.SetText(profile.characterInfo.bodyType.gameObject.name);
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            foreach (Character c in profile.availableCharacters)
            {
                Character character = c;
                void Call()
                {
                    profile.characterInfo.bodyType = character;
                    InitField();
                }
                GameObject go = set.AddButton(character.gameObject.name, Call);

                if (go.TryGetComponent(out Image image) && character != profile.characterInfo.bodyType)
                    image.color *= 0.5f;
            }
        }
    }
}
