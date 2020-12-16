using UnityEngine;

namespace Junk
{
    public class QuaternionTest : MonoBehaviour
    {
        private delegate void ModeList();
        //private event ModeList ModeUpdate;
        private int mode;

        // Use this for initialization
        void Start()
        {
            mode = 0;
            //ModeUpdate += Mode0;
        }

        void OnInteract()
        {
            mode = ++mode % 3;

            switch (mode)
            {
                case 0:
                    //ModeUpdate -= Mode2;
                    //ModeUpdate += Mode0;
                    return;
                case 1:
                    //ModeUpdate -= Mode0;
                    //ModeUpdate += Mode1;
                    return;
                case 2:
                    //ModeUpdate -= Mode1;
                    //ModeUpdate += Mode2;
                    return;
                default:
                    return;
            }
        }

        void Mode0()
        {
            float i, j, k, w, t;

            i = transform.rotation.x;
            j = transform.rotation.y;
            k = transform.rotation.z;
            w = transform.rotation.w;

            t = Time.deltaTime * 0.5f;

            if (Input.GetKey(KeyCode.Alpha1))
                i += t;
            if (Input.GetKey(KeyCode.Alpha2))
                j += t;
            if (Input.GetKey(KeyCode.Alpha3))
                k += t;
            if (Input.GetKey(KeyCode.Alpha4))
                w += t;

            transform.rotation = new Quaternion(i, j, k, w).normalized;
        }

        void Mode1()
        {
            Vector3 offset = Camera.main.gameObject.transform.position - transform.position;
            Quaternion newRot = Quaternion.LookRotation(offset, -Physics.gravity);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRot, Quaternion.Angle(newRot, transform.rotation) * Time.deltaTime * 3f);

            if ((transform.position - Camera.main.gameObject.transform.position).magnitude > 3f)
                transform.position += transform.forward * Mathf.Min(Time.deltaTime * Mathf.Pow((offset).magnitude - 3f, 1.0f), 5f);

            transform.position += Vector3.Project(offset, Physics.gravity) * Time.deltaTime;
        }

        void Mode2()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(-Physics.gravity.normalized, (Camera.main.gameObject.transform.position - transform.position).normalized), 180f * Time.deltaTime);
        }

        // Update is called once per frame
        void Update()
        {
            //modeUpdate();
            Mode1();
        }
    }
}