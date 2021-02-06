using UnityEngine;

namespace MPCore
{
    public class DamageEvent : MonoBehaviour
    {
        public delegate int DamageDelegate(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction);

        public event DamageDelegate OnDamage;

        public int Damage(int damage, GameObject conduit, CharacterInfo instigator, DamageType damageType, Vector3 direction)
        {
            OnDamage?.Invoke(damage, conduit, instigator, damageType, direction);

            return 1;
        }
    }
}
