using MPGUI;
using UnityEngine;

namespace MPCore
{
    public class WindowSpawnEvent : MonoBehaviour
    {
        [SerializeField] Window _window;

        public void SpawnWindow()
        {
            GUIModel guiModel = Models.GetModel<GUIModel>();

            guiModel.OpenWindow?.Invoke(_window);
        }
    }
}