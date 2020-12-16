using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    public class Elevator : MonoBehaviour
    {
        public float elevatorSpeed = 5f;
        public float slowDistance = 5f;
        public ElevatorFloorInfo startingFloor;

        private Rigidbody rb;
        private int direction = 1;
        private ElevatorFloorInfo lastFloor;
        private ElevatorFloorInfo nextFloor;
        private readonly HashSet<ElevatorFloorInfo> requests = new HashSet<ElevatorFloorInfo>();

        public void RequestFloor(ElevatorFloorInfo request)
        {
            if (request == nextFloor)
                request.SetButtons(nextFloor != lastFloor);
            else if (requests.Add(request))
            {
                request.SetButtons(true);

                if (nextFloor == lastFloor)
                    PickNextFloor();
            }
            else if (requests.Remove(request))
                request.SetButtons(false);
        }

        private void PickNextFloor()
        {
            int priority = int.MinValue;

            foreach (ElevatorFloorInfo floor in requests)
            {
                int candidatePriority = (floor.floorNumber - lastFloor.floorNumber) * direction;

                if (candidatePriority > priority)
                {
                    priority = candidatePriority;
                    nextFloor = floor;
                }
            }

            if (Mathf.Sign(nextFloor.floorNumber - lastFloor.floorNumber) != direction)
                direction *= -1;
        }

        void FixedUpdate()
        {
            if (nextFloor != lastFloor)
            {
                Vector3 currentOffset = nextFloor.elevatorPosition.position - transform.position;
                float lastDistance = Vector3.Distance(transform.position, lastFloor.elevatorPosition.position);
                float nextDistance = Vector3.Distance(transform.position, nextFloor.elevatorPosition.position);
                float minDistance = Mathf.Min(lastDistance, nextDistance);
                float speed = Mathf.Clamp(Mathf.Sqrt(minDistance / slowDistance) * elevatorSpeed, 0.1f, elevatorSpeed);
                rb.MovePosition(transform.position + Vector3.ClampMagnitude(currentOffset, speed * Time.fixedDeltaTime));

                if (Vector3.Distance(transform.position, nextFloor.elevatorPosition.position) < 0.01f)
                {
                    rb.MovePosition(nextFloor.elevatorPosition.position);
                    lastFloor = nextFloor;
                    requests.Remove(nextFloor);

                    nextFloor.SetButtons(false);

                    PickNextFloor();
                }
            }
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
            rb.isKinematic = true;
            lastFloor = nextFloor = startingFloor;

            if (startingFloor)
                transform.position = startingFloor.elevatorPosition.position;
        }
    }
}