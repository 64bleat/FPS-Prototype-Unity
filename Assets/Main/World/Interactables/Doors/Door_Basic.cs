using MPCore;
using System;
using UnityEngine;

namespace MPWorld
{
    /// <summary> A script to open and close a hinged door upon interacting with it. </summary>
    public class Door_Basic : MonoBehaviour, IInteractable
    {
        public GameObject _ForcePoint;
        public float _Acceleration = 80f;
        public float _MaxSpeed = 2.0f;
        public float _ClosedAngle = 1.0f;
        public bool _InitiallyOpen = false;
        public AudioClip _OpenSound;
        public AudioClip _CloseSound;

        private readonly StateMachine state = new StateMachine();
        private Action interactAction;
        private Quaternion ClosedRotation;
        private Vector3 ClosedPosition;
        private new Rigidbody rigidbody;
        private float timer;
        private AudioSource audioSource;

        public void Start()
        {
            rigidbody = gameObject.GetComponent<Rigidbody>();

            audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

            ClosedRotation = transform.rotation;
            ClosedPosition = transform.position;

            state.Add(new State("Initialize", Initialize, end: ClosedEnd),
                new State("Closed", ClosedStart, end: ClosedEnd),
                new State("Opening", OpeningStart, OpeningUpdate),
                new State("Opened", null, OpenedUpdate),
                new State("Closing", ClosingStart, ClosingUpdate));
            state.Initialize("Initialize");
        }

        public void Update()
        {
            state.Update();
        }

        //initialize
        private void Initialize()
        {
            if (_InitiallyOpen)
                state.SwitchTo("Opened");
            else
            {
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                interactAction = () => state.SwitchTo("Opening");
            }
        }

        //closed
        private void ClosedStart()
        {
            transform.rotation = ClosedRotation;
            transform.position = ClosedPosition;

            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            interactAction = () => state.SwitchTo("Opening");

            if(_CloseSound)
                audioSource.PlayOneShot(_CloseSound);
        }
        private void ClosedEnd()
        {
            if(_OpenSound)
                audioSource.PlayOneShot(_OpenSound);
        }

        //opening
        private void OpeningStart()
        {
            rigidbody.constraints = RigidbodyConstraints.None;

            interactAction = () => state.SwitchTo("Closing");

            timer = Time.time;
        }

        private void OpeningUpdate()
        {
            if (Time.time - timer < 1.0f && rigidbody.GetPointVelocity(_ForcePoint.transform.position).magnitude < _MaxSpeed)
                rigidbody.AddForceAtPosition(_ForcePoint.transform.forward * _Acceleration, _ForcePoint.transform.position, ForceMode.Acceleration);
            else
                state.SwitchTo("Opened");
        }

        //opened
        private void OpenedUpdate()
        {
            if (Quaternion.Angle(transform.rotation, ClosedRotation) < _ClosedAngle)
                state.SwitchTo("Closed");
        }

        //closing
        private void ClosingStart()
        {
            interactAction = () => state.SwitchTo("Opening");

            timer = Time.time;
        }

        private void ClosingUpdate()
        {
            if (Quaternion.Angle(transform.rotation, ClosedRotation) < _ClosedAngle)
                state.SwitchTo("Closed");
            if (Time.time - timer < 1.0f && rigidbody.GetPointVelocity(_ForcePoint.transform.position).magnitude < _MaxSpeed)
                rigidbody.AddForceAtPosition(-_ForcePoint.transform.forward * _Acceleration, _ForcePoint.transform.position, ForceMode.Acceleration);
            else
                state.SwitchTo("Opened");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.isStatic && !collision.gameObject.GetComponent<Door_Basic>() && (state.IsCurrentState("Opening") || state.IsCurrentState("Closing")))
                timer = 0;
        }

        public void OnInteractHold(GameObject other, RaycastHit hit){}
        public void OnInteractEnd(GameObject other, RaycastHit hit){}
        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            interactAction.Invoke();
        }
    }
}
