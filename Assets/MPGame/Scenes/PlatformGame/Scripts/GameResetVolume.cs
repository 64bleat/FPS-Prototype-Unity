using MPCore;
using UnityEngine;

public class GameResetVolume : MonoBehaviour
{
    private PlatformGameModel _gameModel;

    private void Awake()
    {
        _gameModel = Models.GetModel<PlatformGameModel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_gameModel.gameState == PlatformGameModel.State.Stopped
            && other.TryGetComponent(out Character character)
            && character == _gameModel.currentPlayer.Value)
            _gameModel.OnReset?.Invoke();
    }
}
