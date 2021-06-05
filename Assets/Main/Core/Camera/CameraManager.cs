using UnityEngine;

namespace MPCore
{
    public class CameraManager : MonoBehaviour
    {
        public static GameObject target;

        private static GameObject skycam;
        public static Camera main;

        [SerializeField] private FloatValue fov;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera farCamera;
        [SerializeField] private FloatValue firstPersonFov;
        [SerializeField] private Camera firstPersonCamera;

        private void OnEnable()
        {
            skycam = GameObject.FindGameObjectWithTag("SkyCam");
            main = mainCamera;
            SetMainFOV(fov);
            SetFirstFov(fov);
            fov.callback.AddListener(SetMainFOV);
            firstPersonFov.callback.AddListener(SetFirstFov);
        }

        private void OnDisable()
        {
            fov.callback.RemoveListener(SetMainFOV);
            firstPersonFov.callback.RemoveListener(SetFirstFov);
        }

        private void SetMainFOV(float fov)
        {
            mainCamera.fieldOfView = fov;
            farCamera.fieldOfView = fov;
        }

        private void SetFirstFov(float fov)
        {
            firstPersonCamera.fieldOfView = fov;
        }

        public static void ManualUpdateRot()
        {
            if (target && main)
            {
                main.transform.rotation = target.transform.rotation;

                if(skycam)
                    skycam.transform.rotation = target.transform.rotation;
            }
        }

        public static void ManualUpdatePos()
        {
            if (target && main)
                main.transform.position = target.transform.position;
        }

        public void LateUpdate()
        {
            if (target)
            {
                transform.position = target.transform.position;
                transform.rotation = target.transform.rotation;

                if(skycam)
                    skycam.transform.rotation = target.transform.rotation;
            }
        }
    }
}
