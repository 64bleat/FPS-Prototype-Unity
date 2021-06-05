using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class MutationList : ScriptableObject
    {
        public List<Mutator> available;
        public List<Mutator> selection;
    }
}
