using System.Collections;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// <para>Handles character crouching behaviour.</para>
    /// <para>Shrinks or grows the attached <c>CapsuleCollider</c> to match the desired height</para>
    /// </summary>
    /// <remarks>
    /// <para>Growing tests for overlaps to avoid clipping.</para>
    /// <para>When <c>CharacterInput.autoCrouch</c> is off, the overlap test will not be performed
    ///     once the standing height is reached. When it is on, overlaps while standing will
    ///     make the character crouch automatically.</para>
    /// </remarks>
    public class CharacterCrouch : MonoBehaviour
    {
        static readonly Collider[] _overlapBuffer = new Collider[10];
        static readonly string[] _collisionLayers = { "Default", "Physical", "Player" };

        CharacterInput _characterInput;
        CapsuleCollider _cap;
        CharacterBody _body;
        int _layermask;
        Coroutine _coroutine;

        void Awake()
        {
            _characterInput = GetComponent<CharacterInput>();
            _cap = GetComponent<CapsuleCollider>();
            _body = GetComponent<CharacterBody>();
            _layermask = LayerMask.GetMask(_collisionLayers);

            _body.height.Value = _body.defaultHeight;
            _body.height.Subscribe(OnHeightChanged, true);
            _characterInput.moveState.Subscribe(OnStateChanged);
        }

        private void OnDestroy()
        {
            _body.height.Unsubscribe(OnHeightChanged);
            _characterInput.moveState.Unsubscribe(OnStateChanged);
        }

        void OnStateChanged(DeltaValue<CharacterInput.MoveState> state)
        {
            if (state.newValue == CharacterInput.MoveState.Crouch)
                _body.height.Value = _body.defaultCrouchHeight;
            else if (state.oldValue == CharacterInput.MoveState.Crouch
                && state.newValue != CharacterInput.MoveState.Crouch)
                _body.height.Value = _body.defaultHeight;
                
        }

        void OnHeightChanged(DeltaValue<float> height)
        {
            float desiredCapHeight = height.newValue - _body.defaultStepOffset;

            if(_cap.height > desiredCapHeight)
            {
                if(_coroutine != null)
                    StopCoroutine(_coroutine);

                _coroutine = StartCoroutine(CrouchDown(height.newValue));
            }
            else if(_cap.height < desiredCapHeight)
            {
                if (_coroutine != null)
                    StopCoroutine(_coroutine);

                _coroutine = StartCoroutine(CrouchUp(height.newValue));
            }
        }

        IEnumerator CrouchDown(float desired)
        {
            float dampVelocity = 0f;

            desired -= _body.defaultStepOffset;

            while(_cap.height > desired)
            {
                _cap.height = Mathf.SmoothDamp(_cap.height, desired, ref dampVelocity, 0.05f, 10f, Time.deltaTime);
                yield return null;
            }

            _cap.height = desired;
        }

        IEnumerator CrouchUp(float desired)
        {
            float dampVelocity = 0f;

            desired -= _body.defaultStepOffset;

            while(_cap.height < desired)
            {
                if (IsBlocked(desired))
                    dampVelocity = 0f;
                else
                    _cap.height = Mathf.SmoothDamp(_cap.height, desired, ref dampVelocity, 0.05f, 10f, Time.deltaTime);

                yield return null;
            }

            _cap.height = desired;
        }

        bool IsBlocked(float desired)
        {
            Vector3 position = transform.position;
            Vector3 up = transform.up;
            Vector3 point1 = position + up * (desired - _cap.height / 2);
            int count = Physics.OverlapCapsuleNonAlloc(position, point1, _cap.radius, _overlapBuffer, _layermask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
                if (_overlapBuffer[i].gameObject != gameObject)
                    return true;

            return false;
        }
    }
}
