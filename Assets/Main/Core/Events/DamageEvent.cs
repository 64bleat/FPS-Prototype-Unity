using System;
using UnityEngine;

namespace MPCore
{
    public class DamageEvent : MonoBehaviour
    {
        public delegate int DamageDelegate(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction);

        public event DamageDelegate OnDamage;
        public event Action<DamageTicket> OnHit;

        private CharacterInfo characterInfo;

        private void Awake()
        {
            if (TryGetComponent(out Character character))
                characterInfo = character.characterInfo;
        }

        public void Damage(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction)
        {
            DamageTicket ticket = new DamageTicket()
            {
                damage = damage,
                damageType = damageType,
                instigator = instigator,
                victim = characterInfo,
                conduit = conduit,
                victimBody = gameObject,
                momentum = direction,
                point = transform.position,
                normal = direction
            };

            //OnDamage?.Invoke(damage, conduit, instigator, damageType, direction);
            OnHit?.Invoke(ticket);

            //return 1;
        }
    }

    public struct DamageTicket
    {
        public int damage;
        public DamageType damageType;
        public CharacterInfo instigator;
        public CharacterInfo victim;
        public GameObject conduit;
        public GameObject victimBody;
        public Vector3 momentum;
        public Vector3 point;
        public Vector3 normal;
    }
}
