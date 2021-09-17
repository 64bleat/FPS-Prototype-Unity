using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Character Profile Info
    /// </summary>
    public class CharacterInfo : ScriptableObject
    {
        public int id;
        public string displayName;
        public Character bodyType;
        public int team = 0;

        ///<summary> Creat an instanced clone of this CharacterInfo </summary> 
        public CharacterInfo Clone()
        {
            CharacterInfo clone = Instantiate(this);
            clone.id = clone.GetInstanceID();

            return clone;
        }

    }
}
