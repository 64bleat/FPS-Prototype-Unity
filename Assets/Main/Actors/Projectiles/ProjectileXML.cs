using MPCore;
using MPWorld;
using UnityEngine;

namespace Serialization
{
    [XMLSurrogate(typeof(Projectile))]
    public class ProjectileXML : XMLSurrogate
    {
        public Vector3 velocity;

        public override XMLSurrogate Serialize(object o)
        {
            if(o is Projectile p && p)
            {
                if(p.TryGetComponent(out IGravityUser gu))
                    velocity = gu.Velocity;
            }

            return this;
        }

        public override XMLSurrogate Deserialize(object o)
        {
            if (o is Projectile p && p)
            {
                if (p.TryGetComponent(out IGravityUser gu))
                    gu.Velocity = velocity;
            }

            return this;
        }
    }
}
