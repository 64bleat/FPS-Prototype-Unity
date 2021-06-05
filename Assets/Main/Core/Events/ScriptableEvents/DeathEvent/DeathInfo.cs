using MPCore;
using UnityEngine;

namespace MPCore
{
    public struct DeathInfo
    {
        public GameObject conduit;
        public CharacterInfo instigator;
        public CharacterInfo victim;
        public DamageType damageType;
    }
}
