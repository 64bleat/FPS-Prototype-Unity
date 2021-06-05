using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Mutation behavior
    /// </summary>
    public abstract class Mutator : ScriptableObject
    {
        public string displayName;
        public string description;

        /// <summary>
        /// Called for any GameObject with a Mutable component upon its instantiation
        /// </summary>
        /// <param name="gameObject"></param>
        public abstract void Mutate(GameObject gameObject);
    }
}
