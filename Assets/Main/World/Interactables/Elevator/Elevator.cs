using UnityEngine;

namespace MPWorld
{
    /// <summary>
    /// Generic elevator system
    /// </summary>
    public class Elevator : MonoBehaviour
    {
        [Tooltip("Elevator's traveling speed")]
        public float elevatorSpeed = 5f;
        [Tooltip("Elevator begins to slow down within this distance.")]
        public float slowDistance = 5f;
        public int previousFloor;
        public int currentFloor;
        public FloorInfo[] floors;

        private Rigidbody rb;
        private int direction = 1;

        [System.Serializable]
        public struct FloorInfo
        {
            public delegate void FloorRequestDelegate(bool isRequested);
            public Transform floorTransform;
            public event FloorRequestDelegate OnRequestChanged;
            public bool requested;

            public void SetRequest(bool request)
            {
                if(request != requested)
                    OnRequestChanged?.Invoke(request);

                requested = request;
            }
        }

        void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
            rb.isKinematic = true;
            PickNextFloor();
        }

        void FixedUpdate()
        {
            Vector3 elevatorPosition = transform.position;
            Vector3 desiredPosition = floors[currentFloor].floorTransform.position;
            Vector3 currentOffset = desiredPosition - elevatorPosition;
            float prevDist = Vector3.Distance(elevatorPosition, floors[previousFloor].floorTransform.position);
            float curDist = Vector3.Distance(elevatorPosition, desiredPosition);
            float minDistance = Mathf.Min(prevDist, curDist);
            float distanceFactor = slowDistance == 0 ? 1 : minDistance / slowDistance;
            float speed = Mathf.Clamp(Mathf.Sqrt(distanceFactor) * elevatorSpeed, 0.01f, elevatorSpeed);
            Vector3 deltaPosition = Vector3.ClampMagnitude(currentOffset, speed * Time.fixedDeltaTime);

            if (currentOffset.sqrMagnitude < 0.0001f)
            {
                rb.MovePosition(desiredPosition);
                previousFloor = currentFloor;
                floors[currentFloor].SetRequest(false);

                PickNextFloor();
            }
            else
                rb.MovePosition(elevatorPosition + deltaPosition);
        }

        /// <summary>
        /// Send a floor request to the elevator. Also un-requests floors
        /// when they are not being traveled to.
        /// </summary>
        public void RequestFloor(int floor)
        {
            if (floor != currentFloor)
            {
                floors[floor].SetRequest(!floors[floor].requested);

                if (currentFloor == previousFloor)
                    PickNextFloor();
            }
        }

        /// <summary>
        /// Determines which floor the elevator will move to next. Sets
        /// the component to inactive when the elevator is not moving.
        /// </summary>
        /// <remarks>
        /// Requested floors in the same direction the elevator is moving
        /// are prioritized above floors requiring the elevator to turn around.</remarks>
        private void PickNextFloor()
        {
            int priority = int.MinValue;
            int nextFloor = 0;

            for(int f = 0; f < floors.Length; f++)
            {
                if (f == currentFloor || !floors[f].requested)
                    continue;

                int candidatePriority = (f - previousFloor) * direction;

                if (candidatePriority > priority)
                {
                    priority = candidatePriority;
                    nextFloor = f;
                }
            }

            if (priority != int.MinValue)
            {
                currentFloor = nextFloor;
                enabled = true;
            }
            else enabled = false;

            if (Mathf.Sign(currentFloor - previousFloor) != direction)
                direction *= -1;
        }
    }
}