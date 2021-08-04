using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class GameDropdown : DropdownField
    {
        private GameSelectModel _gameSelectModel;

        private void Awake()
        {
            _gameSelectModel = Models.GetModel<GameSelectModel>();
        }

        protected override void InitField()
        {
            valueText.SetText(_gameSelectModel.game.gameObject.name);
        }

        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            for(int i = 0; i < _gameSelectModel.gameList.Count; i++)
            {
                GameController game = _gameSelectModel.gameList[i];
                void call()
                {
                    _gameSelectModel.game = game;
                    InitField();
                }

                set.AddButton(game.gameObject.name, call);
            }
        }
    }
}
