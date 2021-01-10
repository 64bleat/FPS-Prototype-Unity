using MPGUI;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Used to smooth player view on stairs
    /// </summary>
    public class CharacterCamera : MonoBehaviour
    {
        [HideInInspector] public float stepOffset = 0f;

        private CharacterBody body;

        private void Awake()
        {
            Character character = GetComponentInParent<Character>();

            if (character)
                character.OnPlayerSet += OnPlayerSet;

            body = GetComponentInParent<CharacterBody>();

            PauseManager.Add(OnPause);
        }

        private void OnDestroy()
        {
            Character character = GetComponentInParent<Character>();

            if (character)
                character.OnPlayerSet -= OnPlayerSet;

            PauseManager.Remove(OnPause);
        }

        private void OnPause(bool pause)
        {
            enabled = !pause;
        }

        private void FixedUpdate()
        {
            stepOffset = Mathf.Lerp(stepOffset, 0f, Mathf.Min(1, 12f * Time.fixedDeltaTime));

            transform.localPosition = transform.InverseTransformDirection(body.transform.up) * stepOffset;
        }

        private void OnPlayerSet(bool isPlayer)
        {
            if (isPlayer)
                CameraManager.target = gameObject;
        }
    }
}
