using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Shared fields for Projectiles.
    /// </summary>
    public class ProjectileShared : ScriptableObject
    {
        [Header("Firing")]
        public int fireCount = 1;
        public int fireAngle = 0;
        public float fireAngleChoke = 0;
        public float exitSpeed = 100;
        public float speedDeviation = 0f;
        public float lifeSpan = 0.7f;
        public float lifeSpanDeviation = 0f;
        public bool randomSpeedDeviation = true;
        [Header("Damage")]
        public DamageType damageType;
        public int hitDamage = 10;
        [Header("Physics")]
        public float gravityFactor = 0;
        public float hitMomentumTransfer = 15;
        public float characterHitMomentumScale = 1f;
        public float minimumTransferMass = 15f;
        public float hitFrictionFactor = 0.5f;
        public float bounceScaleMin = 0.125f;
        public float bounceScaleMax = 0.3f;
        public float bounceAngle = 2f;
        [Header("Effects")]
        public Particle wallHitParticle;
        public float hue = 0.5f;
    }
}
