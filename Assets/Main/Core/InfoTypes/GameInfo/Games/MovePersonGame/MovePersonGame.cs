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

        private readonly List<CharacterInfo> loadedBots = new List<CharacterInfo>();
        private readonly Queue<CharacterInfo> deadBots = new Queue<CharacterInfo>();
        //private readonly List<Character> activeCharacters = new List<Character>();

        private InputManager input;
        private GameObject currentPlayer;
        private int liveBotCount = 0;

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();

            if (characterSpawnChannel)
                characterSpawnChannel.Add(OnPlayerSet, false);
            if (onDeath)
                onDeath.Add(OnCharacterDied);

            Console.RegisterInstance(this);
            PauseManager.Add(OnPauseUnPause);
        }

        private void Start()
        {
            if (!currentPlayer)
                Spawn(playerInfo);

            if (botmatchInfo)
                for(int i = 0; i < botmatchInfo.botCount; i++)
                {
                    int repeat = i / botmatchInfo.botRoster.Length;
                    CharacterInfo template = botmatchInfo.botRoster[i % botmatchInfo.botRoster.Length];
                    CharacterInfo botInfo = Instantiate(template);

                    if (repeat > 0)
                        botInfo.displayName = $"{template.displayName} ({repeat})";

                    loadedBots.Add(botInfo);
                    deadBots.Enqueue(botInfo);
                }
        }

        private void Update()
        {
            if (deadBots.Count > 0)
                Spawn(deadBots.Dequeue());
            else if(botmatchInfo && loadedBots.Count < botmatchInfo.botCount)
            {
                int i = loadedBots.Count;
                int repeat = i / botmatchInfo.botRoster.Length;
                CharacterInfo botInfo = Instantiate(botmatchInfo.botRoster[i % botmatchInfo.botRoster.Length]);

                if (repeat > 0)
                    botInfo.name = $"{botInfo.name} {repeat}";

                loadedBots.Add(botInfo);
                deadBots.Enqueue(botInfo);
            }
        }

        private void OnDestroy()
        {
            if (characterSpawnChannel)
                characterSpawnChannel.Remove(OnPlayerSet);
            if (onDeath)
                onDeath.Remove(OnCharacterDied);

            Console.RemoveInstance(this);
            PauseManager.Remove(OnPauseUnPause);
        }

        /// <summary> Called when the game pauses or un-pauses </summary>
        private void OnPauseUnPause(bool paused)
        {
            enabled = !paused;
        }

        /// <summary> Called when a character is designated as the player </summary>
        private void OnPlayerSet(object o)
        {
            if (o is Character c && c.isPlayer)
                currentPlayer = c.gameObject;
        }

        /// <summary> called when a character dies </summary>
        private void OnCharacterDied(DeathEventParameters death)
        {
            // Bot Died
            if (death.victim == playerInfo)
                PlayerDeadStart();
            else if (botmatchInfo)
            {
                liveBotCount--;

                if (liveBotCount < botmatchInfo.botCount)
                {
                    deadBots.Enqueue(death.victim);
                    liveBotCount++;
                }
                else
                    Destroy(death.victim);
            }

            // Display Death HUD Notifications
            try
            {
                if (death.victim == playerInfo)
                {
                    if (death.victim == death.instigator)
                        onShortMessage.Invoke("F");
                    else
                        onShortMessage.Invoke($"You were killed by {death.instigator.displayName}");
                    //else if (death.conduit.TryGetComponent(out Projectile _))
                    //    onShortMessage.Invoke($"You were shot to death by {death.instigator.displayName}");
                    //else if (death.conduit.TryGetComponent(out Collider _))
                    //    onShortMessage.Invoke($"You were smashed into a wall by {death.instigator.displayName}");
                    //else
                    //    onShortMessage.Invoke("You are dead. Not big surprise.");
                }
                else if (death.instigator == playerInfo)
                {
                    onShortMessage.Invoke($"You killed {death.victim.displayName}");

                    //if (death.conduit.TryGetComponent(out Projectile _))
                    //    onShortMessage.Invoke($"You killed {death.victim.displayName}");
                    //else if (death.conduit.TryGetComponent(out Collider _))
                    //    onShortMessage.Invoke($"You crushed {death.victim.displayName}");
                    //else
                    //    onShortMessage.Invoke($"You killed {death.victim.displayName} somehow...");
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
            Spawn(playerInfo);
        }

        /// <summary> Spawn a character into the game. </summary>
        /// <param name="characterInfo"> Character to be assigned to the instantiated body </param>
        private void Spawn(CharacterInfo characterInfo)
        {
            if (playerInfo && playerInfo.bodyType)
            {
                GameObject point = PortaSpawn.stack.Count != 0 ? PortaSpawn.stack.Peek() : SpawnPoint.GetSpawnPoint().gameObject;

                if (currentPlayer && characterInfo == playerInfo)
                    if (currentPlayer.TryGetComponent(out Character ch))
                    {
                        ch.Kill(ch.characterInfo, gameObject, respawnDamageType);
                        input.Unbind("Fire", PlayerDeadEnd);
                    }
                    else
                        Destroy(currentPlayer);

                if (point)
                {
                    GameObject playerNew = Instantiate(playerInfo.bodyType.gameObject, point.transform.position, point.transform.rotation);

                    if (playerNew.TryGetComponent(out Character character))
                    {
                        character.characterInfo = characterInfo;
                        character.SetAsCurrentPlayer(characterInfo == playerInfo);

                        foreach (Inventory inv in spawnInventory)
                            inv.TryPickup(character);

                        if (randomSpawnInventory.Count > 0)
                            randomSpawnInventory[Random.Range(0, Mathf.Max(0, randomSpawnInventory.Count))].TryPickup(character);

                        if (point.TryGetComponentInParent(out PortaSpawn spawnPs))
                            spawnPs.TransferStuff(character);
                    }

                    if(playerNew.TryGetComponentInChildren(out IGravityUser playerGU))
                        if (point.TryGetComponentInParent(out Rigidbody spawnRb))
                            playerGU.Velocity = spawnRb.GetPointVelocity(playerNew.transform.position);
                        else if (point.TryGetComponentInParent(out IGravityUser spawnGu))
                            playerGU.Velocity = spawnGu.Velocity;
                }
                else
                    Debug.LogWarning("No spawn point found!");
            }
        }

        [ConsoleCommand("respawn", "Respawns the player")]
        public void Respawn()
        {
            Spawn(playerInfo);
        }

        [ConsoleCommand("player", "Selects the player GameObject in console")]
        public void TargetPlayer()
        {
            Console.target = currentPlayer;
        }
    }
}
