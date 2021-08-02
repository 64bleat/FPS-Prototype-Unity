using UnityEngine;

namespace MPCore
{
    [RequireComponent(typeof(Camera))]
    public class CameraFOV : MonoBehaviour
    {
        [SerializeField] private FloatValue fov;

        private void OnEnable()
        {
            fov.callback.AddListener(SetFov);
            SetFov(fov);
        }

        private void OnDisable()
        {
            fov.callback.RemoveListener(SetFov);
        }

        private void SetFov(float fov)
        {
            Camera camera = GetComponent<Camera>();

            camera.fieldOfView = fov;
        }
    }
}
