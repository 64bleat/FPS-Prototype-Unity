﻿using MPGUI;
using MPConsole;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class Character : MonoBehaviour
    {
        public List<ResourceValue> resources = new List<ResourceValue>();
        public List<Inventory> inventory = new List<Inventory>();
        public ResourceType hurtResource;
        public bool isPlayer = false;

        // Scriptable Events
        public StringEvent onDisplayHealth;
        public DeathEvent onDeath;
        public ObjectEvent onCharacterSpawn;

        public delegate void SetPlayer(bool isPlayer);
        public delegate void CharacterHit(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction);

        public event SetPlayer OnPlayerSet;
        public event CharacterHit OnHit;

        [HideInInspector] public CharacterInfo characterInfo;

        private ResourceValue health;
        private (GameObject instigator, float time) lastAttacker;
        private (CharacterInfo instigator, float taime) lastAttackedBy;

        public int Health => health?.value ?? 0;

        private void OnEnable()
        {
            AiInterestPoints.interestPoints.Add(this);
            Console.RegisterInstance(this);

            GetHealthResource();
        }

        private void OnDisable()
        {
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

        public void SetAsCurrentPlayer(bool isPlayer)
        {
            this.isPlayer = isPlayer;

            if (isPlayer && onDisplayHealth)
                onDisplayHealth.Invoke(health.value.ToString());

            if (onCharacterSpawn)
                onCharacterSpawn.Invoke(this);

            GetHealthResource();

            OnPlayerSet?.Invoke(isPlayer);
        }

        public int Damage(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction)
        {

            OnHit?.Invoke(damage, conduit, instigator, damageType, direction);

            if (health != null)
            {
                if (instigator && instigator != characterInfo)
                    lastAttackedBy = (instigator, Time.time);

                int initialValue = health.value;

                health.value = Mathf.Min(health.maxValue, health.value - damage);

                if (isPlayer && health.value != initialValue && onDisplayHealth)
                    onDisplayHealth.Invoke(health.value.ToString());

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
                    onDisplayHealth.Invoke(health.value.ToString());

                if (initialValue > 0 && health.value <= 0)
                    Kill(instigator, conduit, null);

                return health.value;
            }
            else
                return 0;
        }

        public void Kill(CharacterInfo instigator, GameObject conduit, DamageType damageType)
        {
           // if (TryGetComponent(out CharacterBody body))
            {
                // Spawn Dead Body
                if (TryGetComponent(out CharacterBody body) &&body.deadBody)
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
                if (lastAttackedBy.instigator && Time.time - lastAttackedBy.taime < 3f)
                    instigator = lastAttackedBy.instigator;

                //Create Death Ticket
                DeathEventParameters ticket;
                ticket.conduit = conduit;
                ticket.damageType = damageType;
                ticket.instigator = instigator;
                ticket.victim = characterInfo;

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
