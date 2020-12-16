using System;
using UnityEngine;

namespace MPCore
{
    public class AIAction : ScriptableObject, IGOAPAction
    {
        public bool isFinal = false;

        [NonSerialized] public GameObject gameObject;

        public string Name => name;
        public bool IsFinal => isFinal;

        public AIAction InstantiateFor(GameObject gameObject)
        {
            AIAction instance = Instantiate(this);

            instance.gameObject = gameObject;
            instance.InstanceAwake();

            return instance;
        }

        public virtual void InstanceAwake()
        {

        }

        public virtual void OnStart()
        {

        }

        public virtual void OnEnd()
        {

        }

        public virtual GOAPStatus Update()
        {
            return GOAPStatus.Continue;
        }

        public virtual float? Priority(IGOAPAction successor)
        {
            return null;
        }
    }
}
