using UnityEngine;

namespace Junk
{
    //This is done by a shader now.

    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        void OnWillRenderObject()
        {
            if (Camera.current != null)
            {
                Vector3 lookPos = transform.position - Camera.current.transform.position;
                transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(lookPos, Physics.gravity));
            }
        }
    }
}
