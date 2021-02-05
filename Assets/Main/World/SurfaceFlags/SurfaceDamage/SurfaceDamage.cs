using MPCore;
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

        private readonly Dictionary<GameObject, float> damageTimes = new Dictionary<GameObject, float>();

        public override void OnTouch(GameObject instigator, CBCollision hit)
        {
            if (instigator.TryGetComponent(out CharacterBody body))
            {
                body.Velocity += (hit.normal - body.cameraSlot.forward) * repelForce;
                body.currentState = CharacterBody.MoveState.Airborne;
            }

            if (instigator.TryGetComponent(out Character character))
            {
                GameObject surface = hit.gameObject;
                Vector3 normal = hit.normal;

                if (damageTimes.TryGetValue(instigator, out float lastDamageTime))
                {
                    if (Time.time - lastDamageTime >= damageDuration)
                    {
                        damageTimes.Remove(instigator);
                        character.Damage(damage, hit.gameObject, character.characterInfo, damageType, normal);
                        damageTimes.Add(instigator, Time.time);
                    }
                }
                else
                {
                    character.Damage(damage, surface, character.characterInfo, damageType, normal);
                    damageTimes.Add(instigator, Time.time);
                }

                if (GameObjectPool.TryGetPool(hitEffect, 1, out GameObjectPool pool))
                    pool.Spawn(hit.point);
            }
        }
    }
}
