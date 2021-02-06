using UnityEngine;

namespace MPWorld
{   
    /// <summary>
    /// Scales a child object locally to a parent velocity
    /// </summary>
    public class VelocityTrail : MonoBehaviour
    {
        [Tooltip("Forward scale muliplied by parent velocity")]
        public float velocityScale = 0.01f;

        private IGravityUser body;

        private void Awake()
        {
            body = GetComponentInParent<IGravityUser>();
        }

        private void Update()
        {
            transform.localScale = new Vector3(1, 1, Mathf.Lerp(0, 6, body.Velocity.magnitude * velocityScale));
        }
        
    }
}
