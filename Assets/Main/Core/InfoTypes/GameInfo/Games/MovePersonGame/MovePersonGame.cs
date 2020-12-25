using MPConsole;
using MPWorld;
using System.Collections.Generic;
using UnityEngine;

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
        [HideInInspector] public GameObject player;
        private readonly StateMachine state = new StateMachine();

        private int botCount = 0;

        [ConsoleCommand("respawn", "Respawns the player")]
        public void Respawn()
        {
            Spawn(playerInfo);
        }

        [ConsoleCommand("player", "set console target to the player")]
        public void TargetPlayer()
        {
            Console.target = player;
        }

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();

            if (characterSpawnChannel)
                characterSpawnChannel.Add(SetCharacterSpawn, false);
            if (onDeath)
                onDeath.Add(OnCharacterDied);

            Console.RegisterInstance(this);
        }

        private void OnDestroy()
        {
            if (characterSpawnChannel)
                characterSpawnChannel.Remove(SetCharacterSpawn);
            if (onDeath)
                onDeath.Remove(OnCharacterDied);

            Console.RemoveInstance(this);
        }

        private void Start()
        {
            state.Add(new State("PlayerAlive", PlayerAliveStart));
            state.Add(new State("PlayerDead", PlayerDeadStart, end: PlayerDeadEnd));
            state.Initialize("PlayerAlive");

            if(!player)
             Spawn(playerInfo);

            if(botmatchInfo)
                while(botCount < botmatchInfo.botCount)
                {
                    Spawn(playerInfo, false);
                    botCount++;
                }
        }

        private void SetCharacterSpawn(object o)
        {
            CharacterSpawn(o as Character);
        }

        private void SetCharacterDied(object o)
        {
            OnCharacterDied((DeathEventParameters)o);
        }

        private void CharacterSpawn(Character c)
        {
            if(c && c.isPlayer)
            {
                player = c.gameObject;
                state.SwitchTo("PlayerAlive");
            }
        }

        private void OnCharacterDied(DeathEventParameters death)
        {
            Character target = null;
            Character instigator = null;

            if (death.target)
                death.target.TryGetComponent(out target);

            if (death.instigator)
                death.instigator.TryGetComponent(out instigator);

            //Display Death HUD Notifications
            if (onShortMessage)
            {
                if (target && target.isPlayer)
                {
                    if (death.instigator)
                        if (death.target.Equals(death.instigator))
                            onShortMessage.Invoke("F");
                        else if (death.instigator && death.method && death.method.TryGetComponent(out Projectile _))
                            onShortMessage.Invoke("You were shot to death by " + instigator.characterInfo.displayName);
                        else if (death.instigator && death.method && death.method.TryGetComponent(out Collider _))
                            onShortMessage.Invoke("You were smashed into a wall by " + instigator.characterInfo.displayName);
                        else
                            onShortMessage.Invoke("You are dead. Not big surprise.");

                    state.SwitchTo("PlayerDead", true);
                }
                else if (instigator && instigator.isPlayer)
                {
                    if (target)
                        if (death.method && death.method.TryGetComponent(out Projectile _))
                            onShortMessage.Invoke("You killed " + target.characterInfo.displayName);
                        else if (death.method && death.method.TryGetComponent(out Collider _))
                            onShortMessage.Invoke("You smashed " + target.characterInfo.displayName + " into a wall!");
                        else
                            onShortMessage.Invoke("You killed " + target.characterInfo.displayName + " somehow...");
                }
            }

            if (target && !target.isPlayer && botmatchInfo)
            {
                botCount--;

                while (botCount < botmatchInfo.botCount)
                {
                    Spawn(playerInfo, false);
                    botCount++;
                }
            }
        }

        // PLAYER ALIVE =======================================================
        private void PlayerAliveStart()
        {
            //SpawnPlayer();
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
                GameObject point = PortaSpawn.stack.Count != 0 ? PortaSpawn.stack.Peek() : SpawnPoint.GetSpawnPoint().gameObject;// GameObject.FindGameObjectWithTag("PlayerSpawn");

                if (isPlayer && player)
                    Destroy(player);

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

                    // TODO Iterate through components on spawn point. Check for ISpawnPoint, Call OnSpawn(playerNew)
                }
            }
        }
    }
}
