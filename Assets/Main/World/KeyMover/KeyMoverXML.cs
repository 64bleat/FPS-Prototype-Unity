using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Serialization
{
    [System.Serializable]
    [XMLSurrogate(typeof(KeyMover))]
    public class KeyMoverXML : XMLSurrogate
    {
        [XmlAttribute] public float keyTime;

        public override XMLSurrogate Serialize(object o)
        {
            if(o is KeyMover k && k)
            {
                keyTime = k.keyTime;
            }

            return this;
        }

        public override XMLSurrogate Deserialize(object o)
        {
            if (o is KeyMover k && k)
            {
                k.keyTime = keyTime;
            }

            return this;
        }
    }
}
