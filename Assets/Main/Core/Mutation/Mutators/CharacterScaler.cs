using UnityEngine;

namespace MPCore
{
    public class CharacterScaler : Mutator
    {
        [SerializeField] private float scale;
        public override void Mutate(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out CharacterBody bod))
                bod.ScaleBody(scale);
            else if (gameObject.TryGetComponent(out SpawnPoint sp))
                sp.transform.position += sp.transform.up * (scale - 1);
        }
    }
}
