using UnityEngine;

namespace MPCore
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private GameInfo gameInfo;

        private void Start()
        {
            if (gameInfo)
            {
                GameObject game = Instantiate(gameInfo.game.gameObject, transform);

                Mutation.MutateGame(game);
            }
        }
    }
}
