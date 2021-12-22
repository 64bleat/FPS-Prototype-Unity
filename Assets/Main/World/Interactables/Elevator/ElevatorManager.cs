using System;
using System.Linq;
using UnityEngine;

namespace MPWorld
{
	/// <summary>
	/// Generic elevator system
	/// </summary>
	public class ElevatorManager : MonoBehaviour
	{
		[SerializeField] Transform _elevator;
		[SerializeField] float _elevatorSpeed = 5f;
		[SerializeField] float _slowDistance = 5f;
		[SerializeField] FloorInfo[] _floors;

		Rigidbody _rigidbody;
		int _direction = 1;
		int _previousFloor;
		int _currentFloor;

		[Serializable]
		public class FloorInfo
		{
			public Transform floorTransform;
			public PushButton elevatorButton;
			public PushButton floorButton;
			[NonSerialized] public bool requested;
			[NonSerialized] public int floor;
		}

		void Awake()
		{
			_rigidbody = _elevator.GetComponentInParent<Rigidbody>();
			_rigidbody.isKinematic = true;

			for(int i = 0; i < _floors.Length; i++)
			{
				int f = i;

				_floors[f].floor = f;
				_floors[f].elevatorButton.OnPressed.AddListener(() => RequestFloor(f));
				_floors[f].floorButton.OnPressed.AddListener(() => RequestFloor(f));
			}

			// Snap elevator to nearest floor
			Vector3 position = _elevator.position;
			FloorInfo closestFloor = _floors
				.OrderBy(floor => (floor.floorTransform.position - position).sqrMagnitude)
				.FirstOrDefault();

			if (closestFloor != null)
			{
				_rigidbody.position = closestFloor.floorTransform.position;
				_currentFloor = closestFloor.floor;
				_previousFloor = closestFloor.floor;
			}

			enabled = false;
		}

		void FixedUpdate()
		{
			Vector3 currentPosition = _elevator.position;
			Vector3 nextPosition = _floors[_currentFloor].floorTransform.position;
			Vector3 previousPosition = _floors[_previousFloor].floorTransform.position;
			float prevDist = Vector3.Distance(currentPosition, previousPosition);
			float curDist = Vector3.Distance(currentPosition, nextPosition);
			float minDistance = Mathf.Min(prevDist, curDist);
			float distanceFactor = _slowDistance == 0 ? 1 : minDistance / _slowDistance;
			float speed = Mathf.Clamp(Mathf.Sqrt(distanceFactor) * _elevatorSpeed, 0.01f, _elevatorSpeed);
			Vector3 newPosition = Vector3.MoveTowards(currentPosition, nextPosition, speed * Time.fixedDeltaTime);

			_rigidbody.MovePosition(newPosition);

			if (newPosition == nextPosition)
			{
				_previousFloor = _currentFloor;
				_floors[_currentFloor].elevatorButton.dataValue.Value = false;
				_floors[_currentFloor].floorButton.dataValue.Value = false;
				_floors[_currentFloor].requested = false;

				PickNextFloor();
			}
		}

		void RequestFloor(int floor)
		{
			if (floor != _currentFloor)
			{
				bool request = !_floors[floor].requested;

				_floors[floor].elevatorButton.dataValue.Value = request;
				_floors[floor].floorButton.dataValue.Value = request;
				_floors[floor].requested = request;

				if (_currentFloor == _previousFloor)
					PickNextFloor();
			}
		}

		void PickNextFloor()
		{
			// Closest in direction priority
			FloorInfo nextFloor = _floors
				.Where(floor => floor.floor != _currentFloor && floor.requested)
				.OrderBy(floor => (floor.floor - _previousFloor) * _direction)
				.FirstOrDefault();

			_currentFloor = nextFloor?.floor ?? _currentFloor;
			enabled = nextFloor != null;

			if (Mathf.Sign(_currentFloor - _previousFloor) != _direction)
				_direction *= -1;
		}
	}
}