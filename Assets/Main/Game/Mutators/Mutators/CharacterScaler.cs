using UnityEngine;

namespace MPCore
{
    public class CharacterScaler : Mutator
    {
        [SerializeField] private float scale;

        public override void Activate()
        {
            Messages.Subscribe<Character>(Mutate);
            Messages.Subscribe<SpawnPoint>(MutateSpawn);
        }

        public override void Deactivate()
        {
            Messages.Unsubscribe<Character>(Mutate);
            Messages.Unsubscribe<SpawnPoint>(MutateSpawn);
        }

        void Mutate(Character character)
        {
            if (character.TryGetComponent(out CharacterBody bod))
                bod.ScaleBody(scale);
        }

        void MutateSpawn(SpawnPoint sp)
        {
            sp.transform.position += sp.transform.up * (scale - 1);
        }
    }
}
