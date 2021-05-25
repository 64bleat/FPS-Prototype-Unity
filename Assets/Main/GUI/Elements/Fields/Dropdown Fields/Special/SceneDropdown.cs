using MPCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MPGUI
{
    public class SceneDropdown : DropdownField
    {
        [SerializeField] private GameInfo gameInfo;

        protected override void InitField()
        {
            valueText.SetText(gameInfo.map.displayName);
        }

        protected override void OpenMenu()
        {
            int count = gameInfo.sceneList.Count;
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            for(int i = 0; i < count; i++)
            {
                SceneInfo si = gameInfo.sceneList[i];
                void call ()
                {
                    gameInfo.map = si;
                    InitField();
                }

                set.AddButton(si.displayName, call);
            }
            
            base.OpenMenu();
        }
    }
}
