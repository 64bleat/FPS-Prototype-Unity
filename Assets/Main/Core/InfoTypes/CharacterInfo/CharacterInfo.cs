using MPCore;
using UnityEngine;

public class CharacterInfo : ScriptableObject
{
    public int id;
    public string displayName;
    public Character bodyType;

    public CharacterInfo TempClone()
    {
        CharacterInfo clone = Instantiate(this);
        clone.id = clone.GetInstanceID();

        return clone;
    }

}
