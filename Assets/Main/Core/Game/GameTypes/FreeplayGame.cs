using MPConsole;
using MPWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = MPConsole.Console;
using Random = UnityEngine.Random;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class FreeplayGame : Game
    {   
        public GameInfo botmatchInfo;
        public Scoreboard scoreboard;

        protected CharacterInfo loadedPlayerInfo;
        private readonly List<CharacterInfo> loadedBotInfo = new List<CharacterInfo>();
        private readonly Queue<CharacterInfo> deadBots = new Queue<CharacterInfo>();
        private readonly HashSet<CharacterInfo> liveBots = new HashSet<CharacterInfo>();
        private readonly List<CharacterInfo> orphanedBots = new List<CharacterInfo>();

        private InputManager input;
        private GameObject currentPlayer;

        private void Awake()
        {
            // References
            input = GetComponentInParent<InputManager>();

            characterSpawnChannel.Add(OnSpawn);
            onDeath.Add(OnDeath);

            Console.AddInstance(this);
            PauseManager.AddListener(OnPause);

            // Clear Scoreboard data
            scoreboard.Clear();

            // Register player instance
            PlayerJoin();

            // Register bot instances
            if (botmatchInfo)
                while (loadedBotInfo.Count < botmatchInfo.botCount)
                    BotJoin();
        }

        private void Start()
        {
            Spawn(loadedPlayerInfo);
        }

        private void OnDestroy()
        {
            if (characterSpawnChannel)
                characterSpawnChannel.Remove(OnSpawn);
            if (onDeath)
                onDeath.Remove(OnDeath);

            Console.RemoveInstance(this);
            PauseManager.RemoveListener(OnPause);
        }

        private void Update()
        {
            if (botmatchInfo)
            {
                // Bot count may change at any time
                while (loadedBotInfo.Count < botmatchInfo.botCount)
                    BotJoin();

                // Attempt to spawn one bot per frame
                if (liveBots.Count < botmatchInfo.botCount)
                    if (Spawn(deadBots.Peek()))
                        deadBots.Dequeue();
            }
        }

        private void PlayerJoin()
        {
            loadedPlayerInfo = playerInfo.Clone();

            scoreboard.AddCharacter(loadedPlayerInfo);
        }

        private void BotJoin()
        {
            int i = loadedBotInfo.Count;
            int repeat = i / botmatchInfo.botList.Count;
            CharacterInfo template = botmatchInfo.botList[i % botmatchInfo.botList.Count];
            CharacterInfo botInfo = template.Clone();

            // Create a numbered name for duplicate characters
            if (repeat > 0)
            {
                botInfo.name = $"{botInfo.name} {repeat}";
                botInfo.displayName = $"{botInfo.displayName} {repeat}";
            }

            scoreboard.AddCharacter(botInfo);
            loadedBotInfo.Add(botInfo);
            deadBots.Enqueue(botInfo);
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
                        currentPlayer = go;

                    if (!character.characterInfo)
                    {
                        if (character.isPlayer)
                            character.characterInfo = loadedPlayerInfo;
                        else
                            foreach (CharacterInfo info in loadedBotInfo)
                                if (info.displayName.Equals( character.gameObject.name))
                                    character.characterInfo = info;

                        if (!character.characterInfo)
                        {
                            BotJoin();
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
            if (death.victim == loadedPlayerInfo)
                OnPlayerDeath();
            else if (botmatchInfo)
            {
                liveBots.Remove(death.victim);

                if (liveBots.Count + deadBots.Count < botmatchInfo.botCount)
                    deadBots.Enqueue(death.victim);
                else
                    orphanedBots.Add(death.victim);
            }

            // Display Death HUD Notifications
            MessageEventParameters message = default;
            message.color = Color.black;

            try
            { 
                if (death.victim == loadedPlayerInfo)
                {
                    if (death.victim == death.instigator)
                        message.message = "F";
                    else
                        message.message = $"You were killed by {death.instigator.displayName}";

                    message.color = Color.black;
                    message.bgColor = Color.white;
                }
                else if (death.instigator == loadedPlayerInfo)
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

            // Register to scoreboard.
            scoreboard.AddKill(death.instigator, death.victim);
        }

        /// <summary> Called when the player dies </summary>
        private void OnPlayerDeath()
        { 
            input.Bind("Fire", OnPlayerSpawned, this);
        }

        /// <summary> Called when the player is ready to spawn </summary>
        private void OnPlayerSpawned()
        {
            if(Spawn(loadedPlayerInfo))
                input.Unbind("Fire", OnPlayerSpawned);
        }

        /// <summary> Spawn a character into the game. </summary>
        /// <param name="characterInfo"> Character to be assigned to the instantiated body </param>
        private bool Spawn(CharacterInfo characterInfo)
        {
            SpawnPoint spawn = SpawnPoint.GetRandomSpawnPoint();
            bool isPlayer = characterInfo == loadedPlayerInfo;

            if (!spawn)
                return false;

            // Kill if the character is aready in play
            if (currentPlayer && isPlayer)
                if (currentPlayer.TryGetComponent(out Character ch))
                {
                    ch.Kill(ch.characterInfo, gameObject, respawnDamageType);
                    input.Unbind("Fire", OnPlayerSpawned);
                }
                else
                    Destroy(currentPlayer);

            GameObject reference = (characterInfo.bodyType ? characterInfo : loadedPlayerInfo).bodyType.gameObject;
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
            Spawn(loadedPlayerInfo);
        }

        [ConsoleCommand("player", "Selects the player GameObject in console")]
        public void TargetPlayer()
        {
            Console.target = currentPlayer;
        }
    }
}
