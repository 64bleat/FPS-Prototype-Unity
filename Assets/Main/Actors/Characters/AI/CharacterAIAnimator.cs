using MPConsole;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = MPConsole.Console;

namespace MPCore.AI
{
    [ContainsConsoleCommands]
    public class CharacterAIAnimator : MonoBehaviour
    {
        static readonly string[] _layers = { "Default", "Physical", "Player" };

        public float viewAngle = 45;
        [NonSerialized] public TargetInfo moveTarget;
        [NonSerialized] public TargetInfo visualTarget;
        [NonSerialized] public TargetInfo mentalTarget;
        [NonSerialized] public TargetInfo touchTarget;
        [NonSerialized] public int layerMask;
        [NonSerialized] public readonly List<Vector3> path = new List<Vector3>();

        GameModel _gameModel;
        Character _character;
        Animator _animator;
        DamageEvent _damage;

        private void Awake()
        {
            _gameModel = Models.GetModel<GameModel>();
            _character = GetComponent<Character>();
            _animator = GetComponent<Animator>();
            _damage = GetComponent<DamageEvent>();
            layerMask = LayerMask.GetMask(_layers);

            Console.AddInstance(this);

            _damage.OnHit += OnHit;    
            _character.OnInitialized.AddListener(Initialize);
            _gameModel.isPaused.Subscribe(SetPaused);
        }

        private void OnEnable()
        {
            _animator.enabled = true;
        }

        private void OnDisable()
        {
            _animator.enabled = false;
        }

        private void OnDestroy()
        {
            _gameModel.isPaused.Unsubscribe(SetPaused);
            Console.RemoveInstance(this);

            _damage.OnHit -= OnHit;
        }

        private void Update()
        {
            for (int i = 1; i < path.Count; i++)
                Debug.DrawLine(path[i - 1], path[i], Color.Lerp(Color.magenta, Color.cyan, (float)i / path.Count), 0.25f, true);
        }

        private void Initialize(bool isPlayer)
        {
            _animator.enabled = enabled = !isPlayer;
        }

        private void SetPaused(DeltaValue<bool> paused)
        {
            _animator.enabled = !paused.newValue && !_character.IsPlayer;
        }

        private void OnHit(DamageTicket ticket)
        {
            if (!ticket.instigatorBody || !touchTarget.component)
                return;

            if (ticket.instigatorBody != touchTarget.component.gameObject)
                touchTarget.firstSeen = Time.time;

            touchTarget.mentalPosition = ticket.instigatorBody.transform.position;
            touchTarget.priority = ticket.deltaValue * 10;
            touchTarget.lastSeen = Time.time;
        }

        [ConsoleCommand("posess", "Toggle AI for players")]
        public string Posess()
        {
            if (_character.IsPlayer)
            {
                _animator.enabled = !_animator.enabled;

                return _character.IsPlayer ? _animator.enabled ? "Surrendered control" : "Control restored" : null;
            }
            else 
                return null;
        }
    }
}
