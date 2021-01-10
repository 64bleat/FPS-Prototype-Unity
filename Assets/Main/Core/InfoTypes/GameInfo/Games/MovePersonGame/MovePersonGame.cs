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
        public CharacterInfo playerInfo;
        public BotmatchGameInfo botmatchInfo;
        public ObjectEvent characterSpawnChannel;
        public MessageEvent onShortMessage;
        public DeathEvent onDeath;

        private readonly List<CharacterInfo> loadedBots = new List<CharacterInfo>();
        private readonly Queue<CharacterInfo> deadBots = new Queue<CharacterInfo>();

        private InputManager input;
        private GameObject currentPlayer;
        private readonly StateMachine state = new StateMachine();

        private int botCount = 0;

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();

            state.Add(new State("PlayerAlive"));
            state.Add(new State("PlayerDead", PlayerDeadStart, end: PlayerDeadEnd));
            state.Initialize("PlayerAlive");

            if (characterSpawnChannel)
                characterSpawnChannel.Add(SetCharacterSpawn, false);
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
                Spawn(deadBots.Dequeue(), false);
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
                characterSpawnChannel.Remove(SetCharacterSpawn);
            if (onDeath)
                onDeath.Remove(OnCharacterDied);

            Console.RemoveInstance(this);
            PauseManager.Remove(OnPauseUnPause);
        }

        private void OnPauseUnPause(bool paused)
        {
            enabled = !paused;
        }

        private void SetCharacterSpawn(object o)
        {
            CharacterSpawn(o as Character);
        }

        private void CharacterSpawn(Character c)
        {
            if(c && c.isPlayer)
            {
                currentPlayer = c.gameObject;
                state.SwitchTo("PlayerAlive");
            }
        }

        private void OnCharacterDied(DeathEventParameters death)
        {
            // Bot Died
            if (death.victim == playerInfo) 
                state.SwitchTo("PlayerDead", true);
            else if (botmatchInfo)
            {
                botCount--;

                if (botCount < botmatchInfo.botCount)
                {
                    deadBots.Enqueue(death.victim);
                    botCount++;
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
                    else if (death.method.TryGetComponent(out Projectile _))
                        onShortMessage.Invoke($"You were shot to death by {death.instigator.displayName}");
                    else if (death.method.TryGetComponent(out Collider _))
                        onShortMessage.Invoke($"You were smashed into a wall by {death.instigator.displayName}");
                    else
                        onShortMessage.Invoke("You are dead. Not big surprise.");
                }
                else if (death.instigator == playerInfo)
                {
                    if (death.method.TryGetComponent(out Projectile _))
                        onShortMessage.Invoke($"You killed {death.victim.displayName}");
                    else if (death.method.TryGetComponent(out Collider _))
                        onShortMessage.Invoke($"You crushed {death.victim.displayName}");
                    else
                        onShortMessage.Invoke($"You killed {death.victim.displayName} somehow...");
                }
            }
            catch (Exception)
            {
                // Non-Crucial. Just too many null-checks.
            }
        }

        // PLAYER DEAD ========================================================
        private void PlayerDeadStart()
        {

            input.Bind("Fire", PlayerDeadSwitch, this);
        }
        private void PlayerDeadEnd()
        {
            input.Unbind("Fire", PlayerDeadSwitch);
        }
        private void PlayerDeadSwitch()
        {
            Spawn(playerInfo);
            state.SwitchTo("PlayerAlive");
        }

        private void Spawn(CharacterInfo characterInfo, bool isPlayer = true)
        {
            if (playerInfo && playerInfo.bodyType)
            {
                GameObject point = PortaSpawn.stack.Count != 0 ? PortaSpawn.stack.Peek() : SpawnPoint.GetSpawnPoint().gameObject;

                if (isPlayer && currentPlayer)
                    if (currentPlayer.TryGetComponent(out Character ch))
                        ch.Kill(gameObject, ch.characterInfo, gameObject, null);
                    else
                        Destroy(currentPlayer);

                if (point)
                {
                    GameObject playerNew = Instantiate(playerInfo.bodyType.gameObject, point.transform.position, point.transform.rotation);
                    Character character = playerNew.GetComponent<Character>();
                    IGravityUser playerGU = playerNew.GetComponentInChildren<IGravityUser>();
                    Rigidbody spawnRb = point.GetComponentInParent<Rigidbody>();
                    IGravityUser spawnGu = point.GetComponentInParent<IGravityUser>();
                    PortaSpawn spawnPs = point.GetComponentInParent<PortaSpawn>();

                    if (character)
                    {
                        character.characterInfo = characterInfo;
                        character.SetAsCurrentPlayer(isPlayer);

                        //foreach (Inventory i in spawnInventory)
                        //i.TryPickup(character);
                        if (spawnInventory.Count > 0)
                            spawnInventory[Random.Range(0, Mathf.Max(0, spawnInventory.Count))].TryPickup(character);
                    }

                    // Set Player Velocity to Spawn Point Velocity
                    if (playerGU != null)
                        if (spawnRb)
                            playerGU.Velocity = spawnRb.GetPointVelocity(playerNew.transform.position);
                        else if (spawnGu != null)
                            playerGU.Velocity = spawnGu.Velocity;

                    // Add PortaSpawn inventory
                    if (character && spawnPs)
                        spawnPs.TransferStuff(character);
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
