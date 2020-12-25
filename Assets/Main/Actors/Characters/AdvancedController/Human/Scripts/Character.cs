using MPGUI;
using UnityEngine;
using System.Collections.Generic;

namespace MPCore
{
    public class Character : MonoBehaviour
    {
        public List<ResourceItem> resources = new List<ResourceItem>();
        public List<Inventory> inventory = new List<Inventory>();
        public ResourceType hurtResource;
        public bool isPlayer = false;
        public StringEvent onDisplayHealth;
        public DeathEvent onDeath;
        public ObjectEvent onCharacterSpawn;

        [HideInInspector] public CharacterInfo characterInfo;
        public int Health => health?.value ?? 0;

        private (GameObject instigator, float time) lastAttacker;
        private ResourceItem health;

        private void OnEnable()
        {
            AiInterestPoints.interestPoints.Add(this);

            for(int i = 0; health == null && i < resources.Count; i++)
                if(resources[i].resourceType == hurtResource)
                    health = resources[i];
        }

        private void OnDisable()
        {
            AiInterestPoints.interestPoints.Remove(this);
        }


        public void SetAsCurrentPlayer(bool becomePlayer)
        {
            isPlayer = becomePlayer;

            if(TryGetComponent(out CharacterBody body))
            {
                if(becomePlayer)
                    CameraManager.target = body.cameraSlot ? body.cameraSlot.gameObject : gameObject;
                if(body.thirdPersonBody)
                    body.thirdPersonBody.SetActive(!becomePlayer);
                if(body.cameraAnchor && body.cameraAnchor.TryGetComponent(out MeshRenderer mr))
                    mr.enabled = !becomePlayer;
            }

            if(TryGetComponent(out CharacterAI2 ai))
                ai.enabled = !becomePlayer;

            if (TryGetComponent(out InputManager im))
                im.isPlayer = becomePlayer;

            if (TryGetComponent(out CharacterInput ci))
                ci.Restart();

            if (onCharacterSpawn)
                onCharacterSpawn.Invoke(this);

            if (becomePlayer && onDisplayHealth)
                onDisplayHealth.Invoke(health.value.ToString());

        }

        public int Damage(int damage, GameObject instigator, GameObject method, DamageType type)
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
                    Kill(instigator, method, type);

                return health.value;
            }
            else
                return 0;
        }

        public int Heal(int heal, GameObject instigator, GameObject method)
        {
            if (health != null)
            {
                int initialValue = health.value;

                health.value = Mathf.Min(health.maxValue, health.value + heal);

                if (isPlayer && health.value != initialValue && onDisplayHealth)
                    onDisplayHealth.Invoke(health.value.ToString());

                if (initialValue > 0 && health.value <= 0)
                    Kill(instigator, method, null);

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
    }
}
