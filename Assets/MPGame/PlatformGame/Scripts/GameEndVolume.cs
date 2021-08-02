using MPCore;
using UnityEngine;

public class GameEndVolume : MonoBehaviour
{
    private PlatformGameModel _gameModel;

    private void Awake()
    {
        _gameModel = Models.GetModel<PlatformGameModel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_gameModel.gameState == PlatformGameModel.State.Playing
            && other.TryGetComponent(out Character character)
            && character == _gameModel.currentPlayer.Value)
            _gameModel.OnEnd?.Invoke();
    }
}
