using MPWorld;
using UnityEngine;

namespace MPCore
{
    public class InventoryPickup : MonoBehaviour, IInteractable, ITouchable
    {
        [ContextCreateAsset] 
        public Inventory inventory;
        public bool countDownDestroy = false;
        public float lifeTime;
        public bool destroyOnPickup = true;

        private void OnEnable()
        {
            AiInterestPoints.interestPoints.Add(this);
        }

        private void OnDisable()
        {
            AiInterestPoints.interestPoints.Remove(this);
        }

        public void Update()
        {
            if (countDownDestroy && (lifeTime -= Time.deltaTime) <= 0)
                Destroy(gameObject);
        }

        public virtual void OnPickup(GameObject picker)
        {
            if (picker && picker.TryGetComponent(out InventoryContainer container)
                && container.TryPickup(inventory, out _))
                //&& inventory.TryPickup(container, out _))
                //&& InventoryManager.PickUp(container, inventory))
            {
                gameObject.SetActive(false);

                if (inventory.pickupSound && picker.GetComponent<CharacterSound>() is var sound 
                    && sound && sound.pickupSource)
                    sound.pickupSource.PlayOneShot(inventory.pickupSound);

                if(destroyOnPickup)
                    Destroy(gameObject);
            }
        }

        public virtual void OnDropped(GameObject dropper)
        {
            //if (inventory.droppedLifeTime > 0)
            //{
            //    countDownDestroy = true;
            //    lifeTime = inventory.droppedLifeTime;
            //}
        }
        
        public virtual void OnTouch(GameObject instigator, Collision c)
        {
            if (inventory && inventory.pickupOnTouch && instigator)
                OnPickup(instigator);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(inventory && inventory.pickupOnTouch && other)
                OnPickup(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (inventory && inventory.pickupOnTouch && collision != null)
                OnPickup(collision.gameObject);
        }

        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            if (inventory && inventory.pickupOnInteractStart && other)
                OnPickup(other);
        }
        public void OnInteractEnd(GameObject other, RaycastHit hit) { }
        public void OnInteractHold(GameObject other, RaycastHit hit) { }
    }
}
