/******************************************************************************      
 * author: David Martinez Copyright 2020 all rights reserved
 * description: Vector2Lever is like two FloatLevers combined into one,
 *  representing two axes.
 * procedure:
 *  1.  Add a collider to this GameObject that will cover the radius between
 *      leverCenter and leverMax;
 *  2.  Set leverCenter to an empty GameObject representing the center of
 *      the lever (0,0). Set leverMax to an empty GameObject representing
 *      the top of the lever (0, 1).
 *  3.  Value will be calculated by the position of lever relative to
 *      leverCenter and leverMax.
 *  notes:
 *  -   Value will never be greater than a unit vector.
 *  -   Setting value will automatically set the lever to the correct position.
 *****************************************************************************/
using UnityEngine;
using MPCore;

namespace MPCore
{
    public class Vector2Lever : MonoBehaviour , IInteractable, IVector2Value
    {
        public Transform lever;
        public Transform leverCenter;
        public Transform leverMax;
        public float decayDelay = 0f;
        public float rateOfDecay = 0f;

        private float lastInteractTime = 0f;
        private Vector2 value;
        private float maxMag;

        public Vector2 Value
        {
            get => value;

            set
            {
                value = Vector2.ClampMagnitude(value, 1);
                lever.position = leverCenter.position - leverCenter.forward * value.x * maxMag + leverCenter.right * value.y * maxMag;
                this.value = value;
            }
        }

        public void Start()
        {
            maxMag = (leverMax.position - leverCenter.position).magnitude;
        }

        public void Update()
        {
            if (rateOfDecay > 0 && Time.time - lastInteractTime > decayDelay)
                Value = Vector2.Lerp(Value, Value * 0.5f, Time.deltaTime / rateOfDecay);
        }

        public void OnInteractStart(GameObject other, RaycastHit hit) { }
        public void OnInteractHold(GameObject other, RaycastHit hit)
        {
            {// PROJECT TO LEVER PLANE
                Interactor interactor = other.GetComponentInChildren<Interactor>();
                Vector3 interactDirection = (hit.point - interactor.gameObject.transform.position).normalized;
                float a = Vector3.Dot(leverCenter.position - interactor.gameObject.transform.position, leverCenter.transform.up);
                float b = Vector3.Dot(interactDirection, leverCenter.transform.up);

                if (b != 0 && a != 0)
                    hit.point = interactor.gameObject.transform.position + interactDirection * a / b;
            }

            {// POSITION TO VALUE
                Vector3 offset = Vector3.ProjectOnPlane(hit.point - leverCenter.position, leverCenter.up);
                float x = Vector3.Project(offset, -leverCenter.forward).magnitude * Mathf.Sign(Vector3.Dot(offset, -leverCenter.forward));
                float y = Vector3.Project(offset, leverCenter.right).magnitude * Mathf.Sign(Vector3.Dot(offset, leverCenter.right));

                Value = new Vector2(x, y) / maxMag;
            }

            lastInteractTime = Time.time;
        }
        public void OnInteractEnd(GameObject other, RaycastHit hit) { }
    }
}
