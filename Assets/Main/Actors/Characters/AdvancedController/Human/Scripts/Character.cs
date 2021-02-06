using MPConsole;
using MPGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = MPConsole.Console;

namespace MPCore
{
    /// <summary>
    /// Primary component of the entire Character Controller system
    /// </summary>
    [ContainsConsoleCommands]
    public class Character : MonoBehaviour
    {
        public List<ResourceValue> resources = new List<ResourceValue>();
        public ResourceType hurtResource;
        public bool isPlayer = false;

        // Scriptable Events
        public StringEvent onDisplayHealth;
        public DeathEvent onDeath;
        public ObjectEvent onCharacterSpawn;
        public MessageEvent pickupMessager;
        public WeaponSlotEvents inventoryEvents;

        // Events
        public event Action<bool> OnPlayerSet;
        public event Action<DamageTicket> OnHit;

        // Runtime
        [NonSerialized] public CharacterInfo characterInfo;
        [NonSerialized] public ResourceValue health;
        [NonSerialized] public CharacterBody body;

        private (CharacterInfo instigator, float time) lastAttackedBy;

        private void Awake()
        {
            TryGetComponent(out body);
        }

        private void OnEnable()
        {
            if (TryGetComponent(out DamageEvent damage))
                damage.OnDamage += Damage;

            AiInterestPoints.interestPoints.Add(this);
            Console.RegisterInstance(this);

            GetHealthResource();
        }

        private void OnDisable()
        {
            if (TryGetComponent(out DamageEvent damage))
                damage.OnDamage -= Damage;

            AiInterestPoints.interestPoints.Remove(this);
            Console.RemoveInstance(this);            
        }

        public void GetHealthResource()
        {
            for (int i = 0; i < resources.Count; i++)
                if (resources[i].resourceType == hurtResource)
                {
                    health = resources[i];
                    return;
                }
        }

        public void RegisterCharacter()
        {
            GetHealthResource();

            OnPlayerSet?.Invoke(isPlayer);
            onCharacterSpawn.Invoke(gameObject);

            if (isPlayer)
                onDisplayHealth.Invoke($"{health.value,3}");
        }

        private int Damage(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction)
        {
            DamageTicket ticket = new DamageTicket()
            {
                damage = damage,
                damageType = damageType,
                instigator = instigator,
                victim = characterInfo,
                instigatorBody = conduit,
                victimBody = gameObject,
                normal = direction,
                momentum = direction,
                point = transform.position
            };

            OnHit?.Invoke(ticket);

            if (health != null)
            {
                if (instigator && instigator != characterInfo)
                    lastAttackedBy = (instigator, Time.time);

                int initialValue = health.value;

                health.value = Mathf.Min(health.maxValue, health.value - damage);

                if (isPlayer && health.value != initialValue && onDisplayHealth)
                    onDisplayHealth.Invoke($"{health.value,3}");

                if (initialValue > 0 && health.value <= 0)
                    Kill(instigator, conduit, damageType);

                return health.value;
            }
            else
                return 0;
        }

        public int Heal(int value, GameObject owner, CharacterInfo instigator, GameObject conduit)
        {
            if (health != null)
            {
                int initialValue = health.value;

                health.value = Mathf.Min(health.maxValue, health.value + value);

                if (isPlayer && health.value != initialValue && onDisplayHealth)
                    onDisplayHealth.Invoke($"{health.value, 3}");

                if (initialValue > 0 && health.value <= 0)
                    Kill(instigator, conduit, null);

                return health.value;
            }
            else
                return 0;
        }

        public void Kill(CharacterInfo instigator, GameObject conduit, DamageType damageType)
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
            if(TryGetComponent(out InventoryContainer container))
                foreach (Inventory i in container.inventory)
                    if (i.dropOnDeath)
                        i.TryDrop(gameObject, transform.position, transform.rotation, default, out _);

            // Finished off by
            if (lastAttackedBy.instigator && Time.time - lastAttackedBy.time < 3f)
                instigator = lastAttackedBy.instigator;

            //Create Death Ticket
            DeathEventParameters ticket;
            ticket.conduit = conduit;
            ticket.damageType = damageType;
            ticket.instigator = instigator;
            ticket.victim = characterInfo;

            if (onDeath)
                onDeath.Invoke(ticket);

            characterInfo = null;
            Destroy(gameObject);
        }

        [ConsoleCommand("god", "Toggle god mode on players")]
        public string ToggleGodMode()
        {
            if (isPlayer)
                if (health != default)
                    health = default;
                else
                    GetHealthResource();

            if (isPlayer)
                return health != default ? "God mode disabled" : "God mode enabled";
            else
                return null;
        }
    }

    public struct DamageTicket
    {
        public int damage;
        public DamageType damageType;
        public CharacterInfo instigator;
        public CharacterInfo victim;
        public GameObject instigatorBody;
        public GameObject victimBody;
        public Vector3 momentum;
        public Vector3 point;
        public Vector3 normal;
    }
}
