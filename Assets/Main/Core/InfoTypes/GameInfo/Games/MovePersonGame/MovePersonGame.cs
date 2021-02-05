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
    public class MovePersonGame : Game
    {   
        public List<Inventory> spawnInventory;
        public List<Inventory> randomSpawnInventory;
        public CharacterInfo playerInfo;
        public BotmatchGameInfo botmatchInfo;
        public Scoreboard scoreboard;
        public DamageType respawnDamageType;
        public ObjectEvent characterSpawnChannel;
        public MessageEvent onShortMessage;
        public DeathEvent onDeath;

        private CharacterInfo loadedPlayerInfo;
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

            characterSpawnChannel.Add(OnCharacterSpawned);
            onDeath.Add(OnCharacterDied);

            Console.RegisterInstance(this);
            PauseManager.Add(OnPauseUnPause);

            // Clear Scoreboard data
            scoreboard.Clear();

            // Register player instance
            RegisterPlayer();

            // Register bot instances
            if (botmatchInfo)
                while (loadedBotInfo.Count < botmatchInfo.botCount)
                    RegisterBot();
        }

        private void Start()
        {
            // Spawn player on GameStart
            if (!currentPlayer)
                Spawn(loadedPlayerInfo);
            else
                Debug.Log("FAILED");
        }

        private void OnDestroy()
        {
            if (characterSpawnChannel)
                characterSpawnChannel.Remove(OnCharacterSpawned);
            if (onDeath)
                onDeath.Remove(OnCharacterDied);

            Console.RemoveInstance(this);
            PauseManager.Remove(OnPauseUnPause);
        }

        private void Update()
        {
            if (botmatchInfo)
            {
                // Bot count may change at any time
                while (loadedBotInfo.Count < botmatchInfo.botCount)
                    RegisterBot();

                // Attempt to spawn one bot per frame
                if (liveBots.Count < botmatchInfo.botCount)
                    if (Spawn(deadBots.Peek()))
                        deadBots.Dequeue();
            }
        }

        private void RegisterPlayer()
        {
            loadedPlayerInfo = playerInfo.TempClone();

            scoreboard.AddCharacter(loadedPlayerInfo);
        }

        private void RegisterBot()
        {
            int i = loadedBotInfo.Count;
            int repeat = i / botmatchInfo.botRoster.Length;
            CharacterInfo template = botmatchInfo.botRoster[i % botmatchInfo.botRoster.Length];
            CharacterInfo botInfo = template.TempClone();

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
        private void OnPauseUnPause(bool paused)
        {
            enabled = !paused;
        }

        /// <summary> Called when a character is designated as the player </summary>
        private void OnCharacterSpawned(object o)
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
                            RegisterBot();
                            character.characterInfo = deadBots.Dequeue();
                        }
                    }

                    if (!character.isPlayer && character.characterInfo)
                        liveBots.Add(character.characterInfo);
                }
        }

        /// <summary> called when a character dies </summary>
        private void OnCharacterDied(DeathEventParameters death)
        {
            // Bot Died
            if (death.victim == loadedPlayerInfo)
                PlayerDeadStart();
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
                    message.imageColor = Color.white;
                }
                else if (death.instigator == loadedPlayerInfo)
                {
                    message.message = $"You killed {death.victim.displayName}";
                    message.color = Color.black;
                    message.imageColor = Color.white;
                }
                else
                {
                    message.message = $"{death.instigator.displayName} killed {death.victim.displayName}";
                    message.color = Color.grey;
                    message.imageColor = new Color(0f, 0f, 0f, 0.25f);
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
        private void PlayerDeadStart()
        { 
            input.Bind("Fire", PlayerDeadEnd, this);
        }

        /// <summary> Called when the player is ready to spawn </summary>
        private void PlayerDeadEnd()
        {
            if(Spawn(loadedPlayerInfo))
                input.Unbind("Fire", PlayerDeadEnd);
        }

        /// <summary> Spawn a character into the game. </summary>
        /// <param name="characterInfo"> Character to be assigned to the instantiated body </param>
        private bool Spawn(CharacterInfo characterInfo)
        {
            if (loadedPlayerInfo && loadedPlayerInfo.bodyType)
            {
                SpawnPoint spawn = SpawnPoint.GetRandomSpawnPoint();
                //GameObject spawnPoint = PortaSpawn.stack.Count != 0 ? PortaSpawn.stack.Peek() : spawn ? spawn.gameObject : null;

                if (spawn)
                {
                    bool isPlayer = characterInfo == loadedPlayerInfo;

                    // Kill if the character is aready in play
                    if (currentPlayer && isPlayer)
                        if (currentPlayer.TryGetComponent(out Character ch))
                        {
                            ch.Kill(ch.characterInfo, gameObject, respawnDamageType);
                            input.Unbind("Fire", PlayerDeadEnd);
                        }
                        else
                            Destroy(currentPlayer);


                    GameObject playerNew = Instantiate(loadedPlayerInfo.bodyType.gameObject, spawn.transform.position, spawn.transform.rotation);

                    playerNew.name = characterInfo.displayName;

                    if (playerNew.TryGetComponent(out Character character))
                    {
                        character.characterInfo = characterInfo;
                        character.isPlayer = isPlayer;
                        character.RegisterCharacter();

                        foreach (Inventory inv in spawnInventory)
                            inv.TryPickup(character, out _);

                        if (randomSpawnInventory.Count > 0)
                            randomSpawnInventory[Random.Range(0, Mathf.Max(0, randomSpawnInventory.Count))].TryPickup(character, out _);

                        if (spawn.TryGetComponentInParent(out PortaSpawn spawnPs))
                            spawnPs.TransferStuff(character);
                    }

                    // Match SpawnPoint Velocity
                    if(playerNew.TryGetComponentInChildren(out IGravityUser playerGU))
                        if (spawn.TryGetComponentInParent(out Rigidbody spawnRb))
                            playerGU.Velocity = spawnRb.GetPointVelocity(playerNew.transform.position);
                        else if (spawn.TryGetComponentInParent(out IGravityUser spawnGu))
                            playerGU.Velocity = spawnGu.Velocity;

                    spawn.lastSpawnTime = Time.time;

                    return true;
                }
            }

            return false;
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
