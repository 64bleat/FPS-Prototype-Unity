using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    public static class Mutation
    {
        public static UnityEvent<GameObject> gameStart = new UnityEvent<GameObject>();
        public static UnityEvent<GameObject> goStart = new UnityEvent<GameObject>();
        
        public static void LoadMutationList(MutationList list)
        {
            gameStart.RemoveAllListeners();
            goStart.RemoveAllListeners();

            foreach (Mutator m in list.selection)
            {
                goStart.AddListener(m.Mutate);

                if (m is GameMutator gm)
                    gameStart.AddListener(gm.MutateGame);
            }
        }

        public static void MutateGameObject(GameObject gameObject)
        {
            goStart?.Invoke(gameObject);
        }

        public static void MutateGame(GameObject gameObject)
        {
            gameStart?.Invoke(gameObject);
        }
    }
}
