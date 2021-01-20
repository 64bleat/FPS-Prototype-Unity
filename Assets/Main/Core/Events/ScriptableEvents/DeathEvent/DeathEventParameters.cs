using MPCore;
using UnityEngine;

namespace MPCore
{
    public struct DeathEventParameters
    {
        //public GameObject target;
        //public GameObject owner;
        public GameObject conduit;
        public CharacterInfo instigator;
        public CharacterInfo victim;
        public DamageType damageType;
    }
}
