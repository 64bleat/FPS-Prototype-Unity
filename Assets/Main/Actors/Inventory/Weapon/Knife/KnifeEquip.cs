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

        public DamageType damageType;

        private Animator animator;
        private InputManager input;
        private Character character;
        private Transform attackPoint;
        private CharacterInfo instigator;

        private static readonly string[] layerNames = new string[] { "Player", "Default", "Physical" };
        private static int layermask;

        private void Awake()
        {
            Component c = this;

            c.TryGetComponentInChildren(out animator);
            c.TryGetComponentInParent(out input);
            c.TryGetComponentInParent(out character);
            instigator = character.characterInfo;
            

            if (c.TryGetComponentInParent(out CharacterBody cb))
                attackPoint = cb.cameraSlot;

            layermask = LayerMask.GetMask(layerNames);
        }

        private void Update()
        {
            if (input.GetKeyDown("Fire"))
                animator.SetBool("Raise", true);
            else if (input.GetKeyUp("Fire"))
                animator.SetBool("Raise", false);
        }

        public void AnimAttack1()
        {
            if(Physics.SphereCast(attackPoint.position, attackRadius, attackPoint.forward, out RaycastHit hit, attackLength, layermask, QueryTriggerInteraction.Ignore))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;

                if (rb && !rb.isKinematic)
                    rb.AddForceAtPosition(attackPoint.forward * 1000, hit.point, ForceMode.Impulse);
                else if (hit.collider.TryGetComponentInParent(out IGravityUser gu))
                    gu.Velocity += attackPoint.forward * attackForce / gu.Mass;

                if (hit.collider.TryGetComponentInParent(out Character character))
                    character.Damage(damage, gameObject, instigator, damageType, attackPoint.forward);
            }
        }
    }
}
