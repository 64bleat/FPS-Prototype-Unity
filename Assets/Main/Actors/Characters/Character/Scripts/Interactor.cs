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

		TargetInfo GetRaycastTarget()
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
			_target = GetRaycastTarget();

			_target.interactable?.OnInteractStart(_owner, _target.hit);
		}

		void OnInteractHold()
		{
			_target.interactable?.OnInteractHold(_owner, GetRaycastTarget().hit);
		}

		void OnInteractEnd()
		{
			_target.interactable?.OnInteractEnd(_owner, GetRaycastTarget().hit);
			_target = default; 
		}

		void OnDrop()
		{
			if(_container && _container.Inventory.Count > 0)
			{
				TargetInfo target = GetRaycastTarget();
				Vector3 position = target.hit.point;
				Vector3 forward = Vector3.ProjectOnPlane(target.hit.point - transform.position, target.hit.normal);
				Quaternion rotation = Quaternion.LookRotation(forward, _owner.transform.up);
				int index = _container.Inventory.Count - 1;

				_container.Drop(index, position, rotation, target.hit);
			}
		}
	}
}
