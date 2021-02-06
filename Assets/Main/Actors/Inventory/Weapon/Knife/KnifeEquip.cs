#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

using MPWorld;
using UnityEngine;

namespace MPCore
{
    public class KnifeEquip : MonoBehaviour
    {
        public int damage = 40;
        public float attackLength = 1.5f;
        public float attackRadius = 0.125f;
        public float attackForce = 1000f;

        public ProjectileShared shared;
        public AudioClip missSound;

        private int raiseId;

        private Animator animator;
        private InputManager input;
        private Character character;
        private Transform attackPoint;
        private CharacterInfo instigator;
        private AudioSource audio;

        private static readonly string[] layerNames = new string[] { "Player", "Default", "Physical" };
        private static int layermask;

        private void Awake()
        {
            Component c = this;

            c.TryGetComponentInChildren(out animator);
            c.TryGetComponentInParent(out input);
            c.TryGetComponentInParent(out character);
            c.TryGetComponentInParent(out audio);
            instigator = character.characterInfo;
            

            if (c.TryGetComponentInParent(out CharacterBody cb))
                attackPoint = cb.cameraSlot;

            layermask = LayerMask.GetMask(layerNames);
            raiseId = Animator.StringToHash("Raise");
        }

        private void OnEnable()
        {
            RaiseUp();

            input.Bind("Fire", RaiseDown, this, KeyPressType.Down);
            input.Bind("Fire", RaiseUp, this, KeyPressType.Up);

            if (input.GetKey("Fire"))
                RaiseDown();
        }

        private void OnDisable()
        {
            input.Unbind(this);
        }

        private void RaiseDown() => animator.SetBool(raiseId, true);
        private void RaiseUp() => animator.SetBool(raiseId, false);


        public void AnimAttack1()
        {
            if(Physics.SphereCast(attackPoint.position, attackRadius, attackPoint.forward, out RaycastHit hit, attackLength, layermask, QueryTriggerInteraction.Ignore))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;

                if (rb && !rb.isKinematic)
                    rb.AddForceAtPosition(attackPoint.forward * 1000, hit.point, ForceMode.Impulse);
                else if (hit.collider.TryGetComponentInParent(out IGravityUser gu))
                    gu.Velocity += attackPoint.forward * attackForce / gu.Mass;

                if (hit.collider.TryGetComponentInParent(out DamageEvent damageEvent))
                    damageEvent.Damage(damage, gameObject, instigator, shared.damageType, attackPoint.forward);

                // COPYPASTA FROM PROJECTILE.CS !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                HitEffect hitEffect = default;
                SurfaceType surfaceType;

                if (hit.collider.TryGetComponent(out SurfaceFlagObject surface))
                    surfaceType = surface.surfaceType;
                else
                    surfaceType = null;

                foreach (HitEffect surfaceEffect in shared.hitEffects)
                    if (surfaceEffect.surfaceType == surfaceType)
                    {
                        hitEffect = surfaceEffect;
                        break;
                    }

                Vector3 direction = Vector3.ProjectOnPlane(Random.insideUnitSphere, hit.normal);
                Quaternion rotation = Quaternion.LookRotation(direction, hit.normal);

                if (hitEffect.effect)
                {
                    GameObjectPool p = GameObjectPool.GetPool(hitEffect.effect, 100);
                    p.Spawn(hit.point, rotation, null);
                }

                if(hitEffect.hitMark)
                {
                    GameObjectPool p = GameObjectPool.GetPool(hitEffect.hitMark, 100);
                    p.Spawn(hit.point, rotation, null);
                }

                if (hitEffect.hitSound)
                    audio.PlayOneShot(hitEffect.hitSound);
            }
            else
            {
                audio.PlayOneShot(missSound);
            }
        }
    }
}
