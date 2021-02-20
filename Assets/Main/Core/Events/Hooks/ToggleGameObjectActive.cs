using UnityEngine;

namespace MPCore
{
    public class ToggleGameObjectActive : MonoBehaviour
    {
        public void ToggleActive()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
