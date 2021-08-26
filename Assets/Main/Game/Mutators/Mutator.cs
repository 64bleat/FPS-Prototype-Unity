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

        public virtual void Activate() { }
        public virtual void Deactivate() { }
    }
}
