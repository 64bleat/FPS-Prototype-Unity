using MPGUI;
using MPConsole;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class Character : MonoBehaviour
    {
        public delegate void SetPlayer(bool isPlayer);
        public event SetPlayer OnPlayerSet;

        public List<ResourceItem> resources = new List<ResourceItem>();
        public List<Inventory> inventory = new List<Inventory>();
        public ResourceType hurtResource;
        public bool isPlayer = false;
        public StringEvent onDisplayHealth;
        public DeathEvent onDeath;
        public ObjectEvent onCharacterSpawn;

        [HideInInspector] public CharacterInfo characterInfo;

        private ResourceItem health;
        private (GameObject instigator, float time) lastAttacker;

        public int Health => health?.value ?? 0;

        private void OnEnable()
        {
            AiInterestPoints.interestPoints.Add(this);
            Console.RegisterInstance(this);

            // Find Health Resource
            GetHealthResource();
        }

        private void OnDisable()
        {
            AiInterestPoints.interestPoints.Remove(this);
            Console.RemoveInstance(this);
        }

        public void GetHealthResource()
        {
            for (int i = 0; health == null && i < resources.Count; i++)
                if (resources[i].resourceType == hurtResource)
                    health = resources[i];
        }

        public void SetAsCurrentPlayer(bool isPlayer)
        {
            this.isPlayer = isPlayer;

            OnPlayerSet?.Invoke(isPlayer);

            if (isPlayer && onDisplayHealth)
                onDisplayHealth.Invoke(health.value.ToString());

            if (onCharacterSpawn)
                onCharacterSpawn.Invoke(this);
        }

        public int Damage(int damage, GameObject instigator, GameObject weapon, DamageType damageType)
        {
            if (health != null)
            {
                if (instigator && instigator.TryGetComponent(out Character c) && c != this)
                    lastAttacker = (instigator, Time.time);

                int initialValue = health.value;

                health.value = Mathf.Min(health.maxValue, health.value - damage);

                if (isPlayer && health.value != initialValue && onDisplayHealth)
                    onDisplayHealth.Invoke(health.value.ToString());

                if (initialValue > 0 && health.value <= 0)
                    Kill(instigator, weapon, damageType);

                return health.value;
            }
            else
                return 0;
        }

        public int Heal(int value, GameObject healer, GameObject healingPickup)
        {
            if (health != null)
            {
                int initialValue = health.value;

                health.value = Mathf.Min(health.maxValue, health.value + value);

                if (isPlayer && health.value != initialValue && onDisplayHealth)
                    onDisplayHealth.Invoke(health.value.ToString());

                if (initialValue > 0 && health.value <= 0)
                    Kill(healer, healingPickup, null);

                return health.value;
            }
            else
                return 0;
        }

        private void Kill(GameObject instigator, GameObject method, DamageType type)
        {
            if (TryGetComponent(out CharacterBody body))
            {
                // Spawn Dead Body
                if (body.deadBody)
                {
                    GameObject db = Instantiate(body.deadBody, body.cameraAnchor.position, body.cameraAnchor.rotation, null);

                    if(db.TryGetComponent(out Rigidbody rb))
                    {
                        rb.velocity = body.Velocity;
                        rb.mass = body.defaultMass;
                    }

                    if (isPlayer)
                        CameraManager.target = db;

                    Destroy(db, 10);
                }

                // Drop Inventory
                foreach (Inventory i in inventory.ToArray())
                    if (i.dropOnDeath)
                        i.TryDrop(gameObject, transform.position, transform.rotation, default, out _);

                // Finished off by
                if (lastAttacker.instigator && Time.time - lastAttacker.time < 3f)
                    instigator = lastAttacker.instigator;

                //Create Death Ticket
                DeathEventParameters ticket;
                ticket.target = gameObject;
                ticket.instigator = instigator;
                ticket.method = method;
                ticket.damageType = type;

                if (onDeath)
                    onDeath.Invoke(ticket);
            }

            Destroy(gameObject);
        }

        [ConsoleCommand("god", "Toggle god mode on players")]
        public void ToggleGodMode()
        {
            if (isPlayer)
                if (health != default)
                    health = default;
                else
                    GetHealthResource();
        }
    }
}
