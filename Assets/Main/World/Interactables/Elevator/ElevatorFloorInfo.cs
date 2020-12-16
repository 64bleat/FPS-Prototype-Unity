using UnityEngine;

namespace MPWorld
{
    public class ElevatorFloorInfo : MonoBehaviour
    {
        public int floorNumber;
        public Transform elevatorPosition;
        public BoolLever[] floorButtons;

        public void SetButtons(bool value)
        {
            foreach (BoolLever button in floorButtons)
                button.BoolValue = value;
        }
    }
}