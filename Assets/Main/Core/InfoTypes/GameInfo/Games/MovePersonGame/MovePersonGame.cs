using MPConsole;
using MPWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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

        [Header("References")]
        public ObjectEvent characterSpawnChannel;
        public MessageEvent onShortMessage;
        public DeathEvent onDeath;
        public DamageType respawnDamageType;

        private CharacterInfo loadedPlayerInfo;
        private readonly List<CharacterInfo> loadedBotInfo = new List<CharacterInfo>();
        private readonly Queue<CharacterInfo> deadBots = new Queue<CharacterInfo>();
        private readonly HashSet<CharacterInfo> liveBots = new HashSet<CharacterInfo>();
        private readonly List<CharacterInfo> orphanedBots = new List<CharacterInfo>();

        private InputManager input;
        private GameObject currentPlayer;

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();

            if (characterSpawnChannel)
                characterSpawnChannel.Add(OnCharacterSpawned);
            if (onDeath)
                onDeath.Add(OnCharacterDied);

            Console.RegisterInstance(this);
            PauseManager.Add(OnPauseUnPause);

            // Register player instance
            loadedPlayerInfo = Instantiate(playerInfo);

            // Register bot instances
            if (botmatchInfo)
                while (loadedBotInfo.Count < botmatchInfo.botCount)
                    RegisterBot();
        }

        private void Start()
        {
            if (!currentPlayer)
                Spawn(loadedPlayerInfo);
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
                while (loadedBotInfo.Count < botmatchInfo.botCount)
                    RegisterBot();

                if (liveBots.Count < botmatchInfo.botCount)
                    Spawn(deadBots.Dequeue());
            }
        }

        private void RegisterBot()
        {
            int i = loadedBotInfo.Count;
            int repeat = i / botmatchInfo.botRoster.Length;
            CharacterInfo template = botmatchInfo.botRoster[i % botmatchInfo.botRoster.Length];
            CharacterInfo botInfo = Instantiate(template);

            if (repeat > 0)
            {
                botInfo.name = $"{botInfo.name} {repeat}";
                botInfo.displayName = $"{botInfo.displayName} {repeat}";
            }

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
            try
            {
                if (death.victim == loadedPlayerInfo)
                {
                    if (death.victim == death.instigator)
                        onShortMessage.Invoke("F");
                    else
                        onShortMessage.Invoke($"You were killed by {death.instigator.displayName}");
                }
                else if (death.instigator == loadedPlayerInfo)
                {
                    onShortMessage.Invoke($"You killed {death.victim.displayName}");
                }
            }
            catch (Exception)
            {
                // Non-Crucial. Just too many null-checks.
            }
        }

        /// <summary> Called when the player dies </summary>
        private void PlayerDeadStart()
        { 
            input.Bind("Fire", PlayerDeadEnd, this);
        }

        /// <summary> Called when the player is ready to spawn </summary>
        private void PlayerDeadEnd()
        {
            input.Unbind("Fire", PlayerDeadEnd);
            Spawn(loadedPlayerInfo);
        }

        /// <summary> Spawn a character into the game. </summary>
        /// <param name="characterInfo"> Character to be assigned to the instantiated body </param>
        private void Spawn(CharacterInfo characterInfo)
        {
            if (loadedPlayerInfo && loadedPlayerInfo.bodyType)
            {
                GameObject spawnPoint = PortaSpawn.stack.Count != 0 ? PortaSpawn.stack.Peek() : SpawnPoint.GetSpawnPoint().gameObject;
                bool isPlayer = characterInfo == loadedPlayerInfo;

                if (currentPlayer && isPlayer)
                    if (currentPlayer.TryGetComponent(out Character ch))
                    {
                        ch.Kill(ch.characterInfo, gameObject, respawnDamageType);
                        input.Unbind("Fire", PlayerDeadEnd);
                    }
                    else
                        Destroy(currentPlayer);

                if (spawnPoint)
                {
                    GameObject playerNew = Instantiate(loadedPlayerInfo.bodyType.gameObject, spawnPoint.transform.position, spawnPoint.transform.rotation);

                    playerNew.name = characterInfo.displayName;

                    if (playerNew.TryGetComponent(out Character character))
                    {
                        character.characterInfo = characterInfo;
                        character.isPlayer = isPlayer;
                        character.RegisterCharacter();

                        foreach (Inventory inv in spawnInventory)
                            inv.TryPickup(character, verbose: false);

                        if (randomSpawnInventory.Count > 0)
                            randomSpawnInventory[Random.Range(0, Mathf.Max(0, randomSpawnInventory.Count))].TryPickup(character, verbose: false);

                        if (spawnPoint.TryGetComponentInParent(out PortaSpawn spawnPs))
                            spawnPs.TransferStuff(character);
                    }

                    if(playerNew.TryGetComponentInChildren(out IGravityUser playerGU))
                        if (spawnPoint.TryGetComponentInParent(out Rigidbody spawnRb))
                            playerGU.Velocity = spawnRb.GetPointVelocity(playerNew.transform.position);
                        else if (spawnPoint.TryGetComponentInParent(out IGravityUser spawnGu))
                            playerGU.Velocity = spawnGu.Velocity;
                }
                else
                    Debug.LogWarning("No spawn point found!");
            }
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
