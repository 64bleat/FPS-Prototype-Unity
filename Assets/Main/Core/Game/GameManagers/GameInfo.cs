using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class GameInfo : MonoBehaviour
    {
        public List<Inventory> spawnInventory;
        public List<Inventory> randomSpawnInventory;
        public CharacterInfo playerInfo;
        public DamageType respawnDamageType;
        //public ObjectEvent characterSpawnChannel;
        public MessageEvent onShortMessage;
        //public DeathEvent onDeath;
    }
}
