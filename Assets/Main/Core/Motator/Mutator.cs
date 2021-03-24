using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class Mutator : ScriptableObject
    {
        public bool hotSwappable = false;
        
        public virtual void OnActivate(GameObject gameObject)
        {

        }

        public virtual void OnDeactivate(GameObject gameObject)
        {

        }
    }
}
