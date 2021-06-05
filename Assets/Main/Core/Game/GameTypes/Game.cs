using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class Game : MonoBehaviour
    {
        public List<Inventory> spawnInventory;
        public List<Inventory> randomSpawnInventory;
        public CharacterInfo playerInfo;
        public DamageType respawnDamageType;
        public ObjectEvent characterSpawnChannel;
        public MessageEvent onShortMessage;
        public DeathEvent onDeath;
    }
}
