using UnityEngine;

namespace MPCore
{
    public class Interactor : MonoBehaviour
    {
        public float interactDistance = 3f;

        private GameObject owner;
        private TargetInfo target;
        private Character character;
        private int layerMask;

        private void Awake()
        {
            InputManager input = GetComponentInParent<InputManager>();

            character = GetComponentInParent<Character>();
            owner = character.gameObject;
            layerMask = LayerMask.GetMask("Default", "Physical", "Interactable");

            input.Bind("Interact", OnInteractStart, this, KeyPressType.Down);
            input.Bind("Interact", OnInteractHold, this, KeyPressType.Held);
            input.Bind("Interact", OnInteractEnd, this, KeyPressType.Up);
            input.Bind("Drop", OnDrop, this, KeyPressType.Down);
        }

        private struct TargetInfo
        {
            public RaycastHit hit;
            public Collider collider;
            public GameObject gameObject;
            public IInteractable interactable;
            public float startTime;

            public TargetInfo(RaycastHit hit)
            {
                this.hit = hit;
                collider = hit.collider;
                gameObject = collider ? collider.gameObject : null;
                interactable = gameObject ? gameObject.GetComponent<IInteractable>() : null;
                startTime = Time.time;
            }
        }

        private TargetInfo GetInteractPosition()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactDistance, layerMask))
            {
                hit.point = transform.position + transform.forward * interactDistance;
                hit.distance = interactDistance;
                hit.normal = owner.transform.up;
            }

            return new TargetInfo(hit);
        }

        private void OnInteractStart()
        {
            target = GetInteractPosition();

            target.interactable?.OnInteractStart(owner, target.hit);
        }

        private void OnInteractHold()
        {
            target.interactable?.OnInteractHold(owner, GetInteractPosition().hit);
        }

        private void OnInteractEnd()
        {
            target.interactable?.OnInteractEnd(owner, GetInteractPosition().hit);
            target = default; 
        }

        private void OnDrop()
        {
            if(character && character.inventory.Count > 0)
            {
                TargetInfo t = GetInteractPosition();
                Vector3 position = t.hit.point;
                Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(t.hit.point - transform.position, t.hit.normal), owner.transform.up);
                int index = character.inventory.Count - 1;

                InventoryManager.Drop(character.inventory, index, position, rotation, owner, t.hit);
            }
        }
    }
}
