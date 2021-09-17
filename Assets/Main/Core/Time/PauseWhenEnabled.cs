using MPCore;
using UnityEngine;

namespace MPGUI
{
    /// <summary>
    /// Pauses the game while this GameObject is enabled
    /// </summary>
    public class PauseWhenEnabled : MonoBehaviour
    {
        GameModel _gameModel;

        private void Awake()
        {
            _gameModel = Models.GetModel<GameModel>();
        }

        private void OnEnable()
        {
            //PauseManager.Push(this);
            _gameModel.pauseTickets.Value++;
        }

        private void OnDisable()
        {
            ///PauseManager.Pull(this);
            _gameModel.pauseTickets.Value--;
        }
    }
}
