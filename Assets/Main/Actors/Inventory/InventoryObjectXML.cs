using MPCore;

namespace Serialization
{
    [XMLSurrogate(typeof(InventoryPickup))]
    public class InventoryObjectXML : XMLSurrogate
    {
        public bool enableCountDown;
        public float lifeTime;

        public override XMLSurrogate Serialize(object o)
        {
            InventoryPickup i = o as InventoryPickup;

            if(i)
            {
                enableCountDown = i.countDownDestroy;
                lifeTime = i.lifeTime;
            }

            return this;
        }

        public override XMLSurrogate Deserialize(object o)
        {
            InventoryPickup i = o as InventoryPickup;

            if(i)
            {
                i.countDownDestroy = enableCountDown;
                i.lifeTime = lifeTime;
            }

            return this;
        }
    }
}
