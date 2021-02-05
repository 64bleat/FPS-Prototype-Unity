using MPGUI;
using System;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Used to smooth player view on stairs
    /// </summary>
    public class CharacterCamera : MonoBehaviour
    {
        public float stepDampTime = 0.1f;

        [NonSerialized] public float stepOffset = 0f;

        private CharacterBody body;
        private Character character;

        private void Awake()
        {
            character = GetComponentInParent<Character>();
            body = GetComponentInParent<CharacterBody>();

            character.OnPlayerSet += OnPlayerSet;
            PauseManager.Add(OnPause);
        }

        private void OnDestroy()
        {
            character.OnPlayerSet -= OnPlayerSet;
            PauseManager.Remove(OnPause);
        }
        private float velocity = 0f;

        private void FixedUpdate()
        {
            stepOffset = Mathf.SmoothDamp(stepOffset, 0f, ref velocity, stepDampTime, float.MaxValue, Time.fixedDeltaTime);
            transform.localPosition = transform.InverseTransformDirection(body.transform.up) * stepOffset;
        }

        private void OnPause(bool pause)
        {
            enabled = !pause;
        }

        private void OnPlayerSet(bool isPlayer)
        {
            if (isPlayer)
                CameraManager.target = gameObject;
        }
    }
}
