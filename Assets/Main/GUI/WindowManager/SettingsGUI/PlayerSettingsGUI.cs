using MPCore;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class PlayerSettingsGUI : ScriptableObject
    {
        public DropdownSpawn spawner;
        public CharacterInfo characterInfo;
        public Character[] availableCharacters;

        public void OpenPlayerSelectionDropdown(RectTransform button)
        {
            GUIButtonSet set = spawner.SpawnDropdown(button);

            foreach (Character c in availableCharacters)
            {
                Character character = c;
                Image image = set.AddButton(c.gameObject.name, () => characterInfo.bodyType = character).GetComponent<Image>();

                if (image && !c.Equals(characterInfo.bodyType))
                    image.color *= 0.5f;
            }
        }
    }
}
