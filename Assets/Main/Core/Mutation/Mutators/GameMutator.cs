using UnityEngine;

namespace MPCore
{
    public abstract class GameMutator : Mutator
    {
        public abstract void MutateGame(GameObject game);
    }
}
