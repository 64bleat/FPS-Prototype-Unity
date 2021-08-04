using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MPCore
{
    public class GameModel : Models
    {
        public enum State { Ready, Playing, End }
        public ValueEvent<State> state = new ValueEvent<State>();
        public UnityEvent GameLoaded = new UnityEvent();
        public UnityEvent GameStart = new UnityEvent();
        public UnityEvent GameEnd = new UnityEvent();
        public UnityEvent GameClosed = new UnityEvent();
        public UnityEvent<DeathInfo> CharacterDied = new UnityEvent<DeathInfo>();
        public UnityEvent<CharacterInfo, bool> OnPlayerConnected = new UnityEvent<CharacterInfo, bool>();
        public UnityEvent<CharacterInfo> SpawnCharacter = new UnityEvent<CharacterInfo>();
        public UnityEvent<Character> OnCharacterSpawned = new UnityEvent<Character>();
        public UnityEvent<(CharacterInfo scorer, int score)> CharacterScored = new UnityEvent<(CharacterInfo, int)>();
    }
}
