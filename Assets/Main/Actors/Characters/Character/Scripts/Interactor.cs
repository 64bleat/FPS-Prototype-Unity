using MPWorld;
using UnityEngine;

namespace MPCore
{
    public class Interactor : MonoBehaviour
    {
        static readonly string[] _layerNames = new string[] { "Default", "Physical", "Interactable" };

        [SerializeField] float _interactDistance = 3f;

        GameObject _owner;
        TargetInfo _target;
        InventoryManager _container;
        int _layerMask;

        struct TargetInfo
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

        void Awake()
        {
            InputManager input = GetComponentInParent<InputManager>();

            _container = GetComponentInParent<InventoryManager>();
            _owner = _container.gameObject;
            _layerMask = LayerMask.GetMask(_layerNames);

            input.Bind("Interact", OnInteractStart, this, KeyPressType.Down);
            input.Bind("Interact", OnInteractHold, this, KeyPressType.Held);
            input.Bind("Interact", OnInteractEnd, this, KeyPressType.Up);
            input.Bind("Drop", OnDrop, this, KeyPressType.Down);
        }

        TargetInfo GetInteractPosition()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _interactDistance, _layerMask))
            {
                hit.point = transform.position + transform.forward * _interactDistance;
                hit.distance = _interactDistance;
                hit.normal = _owner.transform.up;
            }

            return new TargetInfo(hit);
        }

        void OnInteractStart()
        {
            _target = GetInteractPosition();

            _target.interactable?.OnInteractStart(_owner, _target.hit);
        }

        void OnInteractHold()
        {
            _target.interactable?.OnInteractHold(_owner, GetInteractPosition().hit);
        }

        void OnInteractEnd()
        {
            _target.interactable?.OnInteractEnd(_owner, GetInteractPosition().hit);
            _target = default; 
        }

        void OnDrop()
        {
            if(_container && _container.inventory.Count > 0)
            {
                TargetInfo t = GetInteractPosition();
                Vector3 position = t.hit.point;
                Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(t.hit.point - transform.position, t.hit.normal), _owner.transform.up);
                int index = _container.inventory.Count - 1;

                _container.Drop(index, position, rotation, t.hit);
            }
        }
    }
}
