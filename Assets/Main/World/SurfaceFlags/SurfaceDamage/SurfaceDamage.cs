﻿using MPCore;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    public class SurfaceDamage : SurfaceFlag
    {
        public int damage;
        public float damageDuration = 0.5f;
        public DamageType damageType;

        private readonly Dictionary<GameObject, float> damageTimes = new Dictionary<GameObject, float>();

        public override void OnTouch(GameObject target, Collision hit)
        {
            if (target && target.GetComponent<Character>() is var character && character)
            {
                GameObject surface = hit?.gameObject ?? null;
                CharacterInfo instigator;

                if (surface && surface.TryGetComponent(out Character ch) && ch.characterInfo)
                    instigator = ch.characterInfo;
                else
                    instigator = null;

                if (damageTimes.TryGetValue(target, out float lastDamageTime))
                {
                    if (Time.time - lastDamageTime >= damageDuration)
                    {
                        damageTimes.Remove(target);
                        character.Damage(damage, surface, instigator, damageType, hit.GetContact(0).normal);
                        damageTimes.Add(target, Time.time);
                    }
                }
                else
                {
                    character.Damage(damage, surface, instigator, damageType, hit.GetContact(0).normal);
                    damageTimes.Add(target, Time.time);
                }
            }
        }
    }
}