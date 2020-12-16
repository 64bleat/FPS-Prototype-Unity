using System.Collections.Generic;
using UnityEngine;
using System;

namespace MPCore
{
    public class CharacterAI : MonoBehaviour
    {
        //inspector
        public List<AIAction> actionsToLoad;

        //public 
        [NonSerialized] public GameObject target = null;
        [NonSerialized] public Vector3[] targetPath = null;
        [NonSerialized] public int targetPathIndex = 0;
        [NonSerialized] public Vector3? destination;
        [NonSerialized] public Vector3 lookDestination;

        //private
        private readonly GOAP goap = new GOAP();

        private void Awake()
        {
            foreach (AIAction action in actionsToLoad)
                goap.AddActions(action.InstantiateFor(gameObject));
        }

        private void Update()
        {
            goap.GOAPUpdate();
        }
    }
}
