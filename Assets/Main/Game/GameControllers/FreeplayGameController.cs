using MPConsole;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = MPConsole.Console;
using Random = UnityEngine.Random;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class FreeplayGameController : GameController
    {   
        private readonly List<CharacterInfo> _loadedPlayers = new List<CharacterInfo>();
        private readonly Queue<CharacterInfo> deadBots = new Queue<CharacterInfo>();
        private readonly HashSet<CharacterInfo> liveBots = new HashSet<CharacterInfo>();
        private readonly List<CharacterInfo> orphanedBots = new List<CharacterInfo>();

        private PlaySettingsModel _gameSelect;
        private Scoreboard _scoreboard;
        protected CharacterInfo _currentPlayerInfo;
        private GameObject _currentPlayer;
        private GameModel _gameModel;
        private InputManager _input;
        private CharacterInfo[] _botList;

        private void Awake()
        {
            _gameSelect = Models.GetModel<PlaySettingsModel>();
            _gameModel = Models.GetModel<GameModel>();
            _scoreboard = Models.GetModel<Scoreboard>();
            _input = GetComponentInParent<InputManager>();

            _gameModel.GameLoaded.AddListener(GameLoaded);
            _gameModel.GameStart.AddListener(GameStart);
            _gameModel.OnPlayerConnected.AddListener(_scoreboard.AddCharacter);
            _gameModel.CharacterDied.AddListener(OnDeath);
            _gameModel.CharacterDied.AddListener(_scoreboard.AddKill);
            PauseManager.AddListener(OnPause);
            Console.AddInstance(this);

            _botList = ResourceLoader.GetResources<CharacterInfo>();
        }

        private void Start()
        {
            Messages.Publish<GameController>(this);
            _gameModel.GameLoaded?.Invoke();
            _gameModel.GameStart?.Invoke();
        }

        private void OnDestroy()
        {
            Console.RemoveInstance(this);
            PauseManager.RemoveListener(OnPause);
            Models.RemoveModel<GameModel>();
        }

        private void Update()
        {
            if (_gameSelect)
            {
                // Bot count may change at any time
                while (_loadedPlayers.Count < _gameSelect.botCount)
                {
                    int index = _loadedPlayers.Count % _botList.Length;
                    ConnectPlayer(_botList[index], false);
                }

                // Attempt to spawn one bot per frame
                if (deadBots.Count > 0 && liveBots.Count < _gameSelect.botCount)
                    if (Spawn(deadBots.Peek()))
                        deadBots.Dequeue();
            }
        }

        void GameLoaded()
        {
            ConnectPlayer(playerInfo, true);

            if (_gameSelect)
                for (int i = 0; i < _gameSelect.botCount; i++)
                {
                    int index = _loadedPlayers.Count % _botList.Length;
                    ConnectPlayer(_botList[index], false);
                }
        }

        void GameStart()
        {
            Spawn(_currentPlayerInfo);
        }

        void ConnectPlayer(CharacterInfo character, bool isPlayer)
        {
            character = character.Clone();

            if (isPlayer)
                _currentPlayerInfo = character;
            else
            {
                int i = _loadedPlayers.Count;
                int repeat = i / _botList.Length;

                if(repeat > 0)
                {
                    character.name = $"{character.name} ({repeat})";
                    character.displayName = $"{character.displayName} ({repeat})";
                }

                _loadedPlayers.Add(character);
                deadBots.Enqueue(character);
            }

            _gameModel.OnPlayerConnected?.Invoke(character, isPlayer);
        }

        /// <summary> Called when the game pauses or un-pauses </summary>
        private void OnPause(bool paused)
        {
            enabled = !paused;
        }

        /// <summary> Called when a character is designated as the player </summary>
        private void OnSpawn(object o)
        {
            if(o is GameObject go)
                if (go.TryGetComponent(out Character character))
                {
                    if (character.isPlayer)
                        _currentPlayer = go;

                    if (!character.characterInfo)
                    {
                        if (character.isPlayer)
                            character.characterInfo = _currentPlayerInfo;
                        else
                            foreach (CharacterInfo info in _loadedPlayers)
                                if (info.displayName.Equals( character.gameObject.name))
                                    character.characterInfo = info;

                        if (!character.characterInfo)
                        {
                            int index = _loadedPlayers.Count % _botList.Length;
                            ConnectPlayer(_botList[index], false);
                            character.characterInfo = deadBots.Dequeue();
                        }
                    }

                    if (!character.isPlayer && character.characterInfo)
                        liveBots.Add(character.characterInfo);
                }
        }

        /// <summary> called when a character dies </summary>
        private void OnDeath(DeathInfo death)
        {
            // Bot Died
            if (death.victim == _currentPlayerInfo)
                OnPlayerDeath();
            else if (_gameSelect)
            {
                liveBots.Remove(death.victim);

                if (liveBots.Count + deadBots.Count < _gameSelect.botCount)
                    deadBots.Enqueue(death.victim);
                else
                    orphanedBots.Add(death.victim);
            }

            // Display Death HUD Notifications
            MessageEventParameters message = default;
            message.color = Color.black;

            try
            { 
                if (death.victim == _currentPlayerInfo)
                {
                    if (death.victim == death.instigator)
                        message.message = "F";
                    else
                        message.message = $"You were killed by {death.instigator.displayName}";

                    message.color = Color.black;
                    message.bgColor = Color.white;
                }
                else if (death.instigator == _currentPlayerInfo)
                {
                    message.message = $"You killed {death.victim.displayName}";
                    message.color = Color.black;
                    message.bgColor = Color.white;
                }
                else
                {
                    message.message = $"{death.instigator.displayName} killed {death.victim.displayName}";
                    message.color = Color.grey;
                    message.bgColor = new Color(0f, 0f, 0f, 0.25f);
                }
            }
            catch (Exception)
            {
                // Non-Crucial. Just too many null-checks.
            }
            finally
            {
                if(!message.Equals(default))
                    onShortMessage.Invoke(message);
            }
        }

        /// <summary> Called when the player dies </summary>
        private void OnPlayerDeath()
        { 
            _input.Bind("Fire", OnPlayerSpawned, this);
        }

        /// <summary> Called when the player is ready to spawn </summary>
        private void OnPlayerSpawned()
        {
            if(Spawn(_currentPlayerInfo))
                _input.Unbind("Fire", OnPlayerSpawned);
        }

        /// <summary> Spawn a character into the game. </summary>
        /// <param name="characterInfo"> Character to be assigned to the instantiated body </param>
        private bool Spawn(CharacterInfo characterInfo)
        {
            SpawnPoint spawn = SpawnPoint.GetRandomSpawnPoint();
            bool isPlayer = characterInfo == _currentPlayerInfo;

            if (!spawn)
                return false;

            // Kill if the character is aready in play
            if (_currentPlayer && isPlayer)
                if (_currentPlayer.TryGetComponent(out Character ch))
                {
                    ch.Kill(ch.characterInfo, gameObject, respawnDamageType);
                    _input.Unbind("Fire", OnPlayerSpawned);
                }
                else
                    Destroy(_currentPlayer);

            GameObject reference = (characterInfo.bodyType ? characterInfo : _currentPlayerInfo).bodyType.gameObject;
            GameObject instance = spawn.Spawn(reference);

            instance.name = characterInfo.displayName;

            if (instance.TryGetComponent(out Character character))
            {
                SetTeam(characterInfo);
                character.characterInfo = characterInfo;
                character.isPlayer = isPlayer;
                character.RegisterCharacter();
            }

            if (instance.TryGetComponent(out InventoryContainer inventory))
            {
                foreach (Inventory inv in spawnInventory)
                    inventory.TryPickup(inv, out _);

                if (randomSpawnInventory.Count > 0)
                    inventory.TryPickup(randomSpawnInventory[Random.Range(0, Mathf.Max(0, randomSpawnInventory.Count))], out _);

                if (spawn.TryGetComponentInParent(out PortaSpawn spawnPs))
                    spawnPs.TransferStuff(inventory);
            }

            spawn.lastSpawnTime = Time.time;

            return true;
        }

        protected virtual void SetTeam(CharacterInfo ci)
        {
            ci.team = -1;
        }

        [ConsoleCommand("respawn", "Respawns the player")]
        public void Respawn()
        {
            Spawn(_currentPlayerInfo);
        }

        [ConsoleCommand("player", "Selects the player GameObject in console")]
        public void TargetPlayer()
        {
            Console.target = _currentPlayer;
        }
    }
}
