using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class GameDropdown : DropdownField
    {
        [SerializeField] private GameInfo gameInfo;

        protected override void InitField()
        {
            valueText.SetText(gameInfo.game.gameObject.name);
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            for(int i = 0; i < gameInfo.gameList.Count; i++)
            {
                Game game = gameInfo.gameList[i];
                void call()
                {
                    gameInfo.game = game;
                    InitField();
                }

                set.AddButton(game.gameObject.name, call);
            }
        }
    }
}
