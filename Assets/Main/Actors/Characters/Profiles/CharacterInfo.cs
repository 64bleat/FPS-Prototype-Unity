using MPCore;
using UnityEngine;

/// <summary>
/// Character Profile Info
/// </summary>
public class CharacterInfo : ScriptableObject
{
    public int id;
    public string displayName;
    public Character bodyType;
    [System.NonSerialized] public int team = 0;

    ///<summary> Creat an instanced clone of this CharacterInfo </summary> 
    public CharacterInfo TempClone()
    {
        CharacterInfo clone = Instantiate(this);
        clone.id = clone.GetInstanceID();

        return clone;
    }

}
