using MPCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    public class SurfaceDamage : SurfaceFlag
    {
        public int damage;
        public float damageDuration = 0.5f;
        public float repelForce = 5f;
        public DamageType damageType;
        public GameObject hitEffect;
        public bool selfDamage = true;

        [NonSerialized] public CharacterInfo instigator = null;

        private readonly Dictionary<GameObject, float> damageTimes = new Dictionary<GameObject, float>();

        public override void OnTouch(GameObject toucher, CBCollision hit)
        {
            if (toucher.TryGetComponent(out CharacterBody body))
            {
                body.Velocity += (hit.normal - body.cameraSlot.forward) * repelForce;
                body.currentState = CharacterBody.MoveState.Airborne;
            }

            if(selfDamage && toucher.TryGetComponent(out MPCore.Character character))
                instigator = character.characterInfo;

            if (toucher.TryGetComponent(out DamageEvent damageEvent))
            {
                GameObject surface = hit.gameObject;
                Vector3 normal = hit.normal;

                if (damageTimes.TryGetValue(toucher, out float lastDamageTime))
                {
                    if (Time.time - lastDamageTime >= damageDuration)
                    {
                        damageTimes.Remove(toucher);
                        damageEvent.Damage(damage, hit.gameObject, instigator, damageType, normal);
                        damageTimes.Add(toucher, Time.time);
                    }
                }
                else
                {
                    damageEvent.Damage(damage, surface, instigator, damageType, normal);
                    damageTimes.Add(toucher, Time.time);
                }

                if (GameObjectPool.TryGetPool(hitEffect, 1, out GameObjectPool pool))
                    pool.Spawn(hit.point);
            }
        }
    }
}
