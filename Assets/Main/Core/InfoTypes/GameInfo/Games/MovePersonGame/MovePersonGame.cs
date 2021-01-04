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

        private InputManager input;
        private GameObject currentPlayer;
        private readonly StateMachine state = new StateMachine();

        private int botCount = 0;

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

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();

            state.Add(new State("PlayerAlive"));
            state.Add(new State("PlayerDead", PlayerDeadStart, end: PlayerDeadEnd));
            state.Initialize("PlayerAlive");

            if (characterSpawnChannel)
                characterSpawnChannel.Add(SetCharacterSpawn, false);
            if (onDeath)
                onDeath.Add(CharacterDied);

            Console.RegisterInstance(this);
            PauseManager.Add(OnPauseUnPause);
        }

        private void Start()
        {
            if (!currentPlayer)
                Spawn(playerInfo);

            if (botmatchInfo)
                while (botCount < botmatchInfo.botCount)
                {
                    Spawn(playerInfo, false);
                    botCount++;
                }
        }

        private void OnDestroy()
        {
            if (characterSpawnChannel)
                characterSpawnChannel.Remove(SetCharacterSpawn);
            if (onDeath)
                onDeath.Remove(CharacterDied);

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

        private void CharacterDied(DeathEventParameters death)
        {
            Character target = null;
            Character instigator = null;

            if (death.target)
                death.target.TryGetComponent(out target);

            if (death.instigator)
                death.instigator.TryGetComponent(out instigator);

            // Bot Died
            if (target)
                if (target.isPlayer) 
                    state.SwitchTo("PlayerDead", true);
                else if (botmatchInfo)
                {
                    botCount--;

                    while (botCount < botmatchInfo.botCount)
                    {
                        Spawn(playerInfo, false);
                        botCount++;
                    }
                }

            // Display Death HUD Notifications
            try
            {
                if (target.isPlayer)
                {
                    if (death.target.Equals(death.instigator))
                        onShortMessage.Invoke("F");
                    else if (death.method.TryGetComponent(out Projectile _))
                        onShortMessage.Invoke("You were shot to death by " + instigator.characterInfo.displayName);
                    else if (death.method.TryGetComponent(out Collider _))
                        onShortMessage.Invoke("You were smashed into a wall by " + instigator.characterInfo.displayName);
                    else
                        onShortMessage.Invoke("You are dead. Not big surprise.");
                }
                else if (instigator.isPlayer)
                {
                    if (target.characterInfo)
                        if (death.method.TryGetComponent(out Projectile _))
                            onShortMessage.Invoke($"You killed {target.characterInfo.displayName}");
                        else if (death.method.TryGetComponent(out Collider _))
                            onShortMessage.Invoke("You smashed " + target.characterInfo.displayName + " into a wall!");
                        else
                            onShortMessage.Invoke("You killed " + target.characterInfo.displayName + " somehow...");
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
    }
}
