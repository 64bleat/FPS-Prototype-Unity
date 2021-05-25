using UnityEngine;

namespace MPCore
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private GameInfo gameInfo;

        private void Awake()
        {
            if(gameInfo)
                Instantiate(gameInfo.game.gameObject, transform);
        }
    }
}
