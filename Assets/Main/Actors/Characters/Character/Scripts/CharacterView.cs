using System;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Used to smooth player view on stairs
    /// </summary>
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] float _stepDampTime = 0.1f;

        [NonSerialized] public float stepOffset = 0f;

        CharacterBody _body;
        Character _character;
        GameModel _gameModel;
        float _dampVelocity = 0f;

        void Awake()
        {
            _character = GetComponentInParent<Character>();
            _body = GetComponentInParent<CharacterBody>();

            _character.OnInitialized.AddListener(Initialize);

            _gameModel = Models.GetModel<GameModel>();
            _gameModel.isPaused.Subscribe(SetPaused);
        }

        void OnDestroy()
        {
            _gameModel.isPaused.Unsubscribe(SetPaused);
        }

        private void FixedUpdate()
        {
            stepOffset = Mathf.SmoothDamp(stepOffset, 0f, ref _dampVelocity, _stepDampTime, float.MaxValue, Time.fixedDeltaTime);
            transform.localPosition = transform.InverseTransformDirection(_body.transform.up) * stepOffset;
        }

        private void Initialize(bool isPlayer)
        {
            if (isPlayer)
                _gameModel.currentView.Value = transform;
        }

        private void SetPaused(DeltaValue<bool> pause)
        {
            enabled = !pause.newValue;
        }
    }
}
