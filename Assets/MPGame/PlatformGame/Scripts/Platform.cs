using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPCore;

[RequireComponent(typeof(Rigidbody))]
public class Platform : MonoBehaviour
{
    [SerializeField] float _downSpeed = 1;
    [SerializeField] float _upSpeed = 1;
    private PlatformGameModel _gameModel;
    private Rigidbody _rigidbody;
    private Vector3 _originPos;
    private Quaternion _originRot;
    private bool _occupied;

    private void Awake()
    {
        _gameModel = Models.GetModel<PlatformGameModel>();
        _gameModel.OnReset.AddListener(OnReset);

        _rigidbody = GetComponent<Rigidbody>();

        _originPos = transform.localPosition;
        _originRot = transform.localRotation;
        _occupied = false;
    }

    private void FixedUpdate()
    {
        if (_occupied)
            _rigidbody.velocity = Vector3.ClampMagnitude(Physics.gravity, _downSpeed);
        else
            _rigidbody.velocity = Vector3.ClampMagnitude(_originPos - _rigidbody.position, _upSpeed);
    }

    private void OnReset()
    {
        StartCoroutine(SmoothReset());
    }

    IEnumerator SmoothReset()
    {
        Vector3 position = transform.position;
        Vector3 endPosition = _originPos;
        Quaternion rotation = transform.rotation;
        Quaternion endRotation = _originRot;
        float duration = Vector3.Distance(position, endPosition) * 0.5f + 0.5f;
        float startTime = Time.time;

        while(true)
        {
            float elapsed = (Time.time - startTime) / duration;

            if (elapsed >= 1f)
                break;

            position = Vector3.Lerp(position, endPosition, elapsed);
            rotation = Quaternion.Slerp(rotation, endRotation, elapsed);

            _rigidbody.MovePosition(position);
            _rigidbody.MoveRotation(rotation);

            yield return null;
        }

        _rigidbody.MovePosition(endPosition);
        _rigidbody.MoveRotation(endRotation);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out Character character))
        { 
            if (_gameModel.gameState == PlatformGameModel.State.Reset)
                _gameModel.OnStart?.Invoke(character);

            _occupied = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent(out Character _))
            _occupied = false;
    }
}
