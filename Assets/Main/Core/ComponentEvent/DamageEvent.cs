using System;
using UnityEngine;

namespace MPCore
{
    public class DamageEvent : MonoBehaviour
    {
        public event Action<DamageTicket> OnHit;

        CharacterInfo characterInfo;

        private void Awake()
        {
            if (TryGetComponent(out Character character))
                characterInfo = character.Info;
        }

        public void Damage(int damage, GameObject instigatorBody, CharacterInfo instigator, DamageType damageType, Vector3 direction)
        {
            DamageTicket ticket = new DamageTicket()
            {
                deltaValue = damage,
                damageType = damageType,
                instigator = instigator,
                victim = characterInfo,
                instigatorBody = instigatorBody,
                victimBody = gameObject,
                momentum = direction,
                point = transform.position,
                normal = direction,
                timeStamp = Time.time
            };

            OnHit?.Invoke(ticket);
        }
    }
}
