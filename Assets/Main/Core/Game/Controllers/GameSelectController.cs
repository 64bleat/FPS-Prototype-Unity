using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Loads the correct game mode from the selection model
    /// </summary>
    public class GameSelectController : MonoBehaviour
    {
        private GameSelectModel _gameSelectModel;

        private void Awake()
        {
            _gameSelectModel = Models.GetModel<GameSelectModel>();
        }

        private void Start()
        {
            GameObject game = Instantiate(_gameSelectModel.game.gameObject, transform);

            Mutation.MutateGame(game);
        }
    }
}
