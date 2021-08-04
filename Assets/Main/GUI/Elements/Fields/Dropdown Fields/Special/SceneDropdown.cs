using MPCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MPGUI
{
    public class SceneDropdown : DropdownField
    {
        private GameSelectModel _gameSelectModel;

        private void Awake()
        {
            _gameSelectModel = Models.GetModel<GameSelectModel>();
        }

        protected override void InitField()
        {
            valueText.SetText(_gameSelectModel.scene.displayName);
        }

        protected override void OpenMenu()
        {
            int count = _gameSelectModel.sceneList.Count;
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            for(int i = 0; i < count; i++)
            {
                Case si = _gameSelectModel.sceneList[i];
                void call ()
                {
                    _gameSelectModel.scene = si;
                    InitField();
                }

                set.AddButton(si.displayName, call);
            }
            
            base.OpenMenu();
        }
    }
}
