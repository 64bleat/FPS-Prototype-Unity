using MPCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MPGUI
{
    public class SceneDropdown : DropdownField
    {
        [SerializeField] private GameSelectModel gameInfo;

        protected override void InitField()
        {
            valueText.SetText(gameInfo.scene.displayName);
        }

        protected override void OpenMenu()
        {
            int count = gameInfo.sceneList.Count;
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);

            for(int i = 0; i < count; i++)
            {
                Case si = gameInfo.sceneList[i];
                void call ()
                {
                    gameInfo.scene = si;
                    InitField();
                }

                set.AddButton(si.displayName, call);
            }
            
            base.OpenMenu();
        }
    }
}
