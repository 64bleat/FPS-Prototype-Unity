using UnityEngine;

namespace MPCore
{
    [RequireComponent(typeof(Camera))]
    public class CameraFOV : MonoBehaviour
    {
        [SerializeField] private bool _firstPersonFov = false;

        GraphicsModel _model;

        private void Awake()
        {
            _model = Models.GetModel<GraphicsModel>();

            if (_firstPersonFov)
                _model.fovFirstPerson.Initialize(OnFovChange);
            else
                _model.fov.Initialize(OnFovChange);
        }

        private void OnDestroy()
        {
            if (_firstPersonFov)
                _model.fovFirstPerson.Initialize(OnFovChange);
            else
                _model.fov.Initialize(OnFovChange);
        }

        private void OnFovChange(DeltaValue<float> fov)
        {
            Camera camera = GetComponent<Camera>();
            camera.fieldOfView = fov.newValue;
        }
    }
}
