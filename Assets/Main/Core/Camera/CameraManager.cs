using UnityEngine;

namespace MPGUI
{
    public class CameraManager : MonoBehaviour
    {
        public static GameObject target;

        private static GameObject skycam;
        public static Camera main;

        private void OnEnable()
        {
            skycam = GameObject.FindGameObjectWithTag("SkyCam");
            main = Camera.main;
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
