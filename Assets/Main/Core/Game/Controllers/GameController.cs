using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class GameController : MonoBehaviour
    {
        public List<Inventory> spawnInventory;
        public List<Inventory> randomSpawnInventory;
        public CharacterInfo playerInfo;
        public DamageType respawnDamageType;
        public MessageEvent onShortMessage;
    }
}
