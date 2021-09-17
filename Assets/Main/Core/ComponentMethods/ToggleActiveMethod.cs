using UnityEngine;

namespace MPCore
{
    public class ToggleActiveMethod : MonoBehaviour
    {
        public void ToggleActive()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
