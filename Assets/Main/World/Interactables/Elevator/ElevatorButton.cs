using UnityEngine;

namespace MPWorld
{
    /// <summary>
    /// Adds elevator functionality to BoolLevers
    /// </summary>
    [RequireComponent(typeof(BoolLever))]
    public class ElevatorButton : MonoBehaviour
    {
        public Elevator elevator;
        public int floor;

        private void OnEnable()
        {
            BoolLever button = GetComponent<BoolLever>();

            elevator.floors[floor].OnRequestChanged += button.SetValue;
        }

        private void OnDisable()
        {
            BoolLever button = GetComponent<BoolLever>();

            elevator.floors[floor].OnRequestChanged -= button.SetValue;
        }
    }
}
