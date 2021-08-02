using UnityEngine;

namespace MPCore
{
    public class CameraManager : MonoBehaviour
    {
        public static GameObject target;

        private static GameObject skycam;
        public static Camera main;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera farCamera;
        [SerializeField] private Camera firstPersonCamera;

        private void OnEnable()
        {
            skycam = GameObject.FindGameObjectWithTag("SkyCam");
            main = mainCamera;
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
