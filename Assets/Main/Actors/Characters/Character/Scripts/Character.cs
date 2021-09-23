using MPConsole;
using MPGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Console = MPConsole.Console;

namespace MPCore
{
    /// <summary>
    /// Primary component of the entire Character Controller system
    /// </summary>
    [ContainsConsoleCommands]
    public class Character : MonoBehaviour
    {

        [SerializeReference] ResourceValue _health = new(100);

        public readonly UnityEvent<bool> OnInitialized = new();

        GUIModel _guiModel;
        GameModel _gameModel;
        CharacterBody _body;
        InventoryManager _inventory;
        DamageEvent _damage;
        DamageTicket _lastAttacker;
        bool _isPlayer;
        CharacterInfo _characterInfo;

        public CharacterInfo Info => _characterInfo;
        public InventoryManager Inventory => _inventory;
        public bool IsPlayer => _isPlayer;
        public ResourceValue Health => _health;

        private void Awake()
        {
            _guiModel = Models.GetModel<GUIModel>();

            _gameModel = Models.GetModel<GameModel>();
            _inventory = GetComponent<InventoryManager>();
            _body = GetComponent<CharacterBody>();
            _damage = GetComponent<DamageEvent>();

            _health.Subscribe(KillCharacter);
            _damage.OnHit += Damage;
        }

        private void OnEnable()
        {
            AIBlackboard.visualTargets.Add(this);
            Console.AddInstance(this);
        }

        private void OnDisable()
        {
            AIBlackboard.visualTargets.Remove(this);
            Console.RemoveInstance(this);            
        }

        public void Initialize(CharacterInfo info, bool isPlayer)
        {
            _characterInfo = info;
            _isPlayer = isPlayer;
            OnInitialized?.Invoke(_isPlayer);

            if (_isPlayer)
                _health.Subscribe(DisplayHealthValue);

            Messages.Publish(this);
        }

        private void Damage(DamageTicket ticket)
        {
            _lastAttacker = ticket;
            _health.Value -= ticket.deltaValue;
        }

        public DeltaValue<int> Heal(int value, GameObject owner, CharacterInfo instigator, GameObject conduit)
        {
            int oldValue = _health.Value;
            _health.Value += value;
            return new DeltaValue<int>(oldValue, _health.Value);
        }

        void DisplayHealthValue(DeltaValue<int> health) => _guiModel.health.Value = health.newValue;
        void KillCharacter(DeltaValue<int> health)
        {
            if(health.oldValue > 0 && health.newValue <= 0)
                Kill(_lastAttacker.instigator, _lastAttacker.instigatorBody, _lastAttacker.damageType);
        }

        public void Kill(CharacterInfo instigator, GameObject conduit, DamageType damageType)
        {
            // Spawn Dead Body
            if (_body.deadBody)
            {
                GameObject db = Instantiate(_body.deadBody, _body.cameraAnchor.position, _body.cameraAnchor.rotation, null);

                if (db.TryGetComponent(out Rigidbody rb))
                {
                    rb.velocity = _body.Velocity;
                    rb.mass = _body.defaultMass;
                }

                if (_isPlayer)
                    //CameraManager.target = db;
                    _gameModel.currentView.Value = db.transform;

                Destroy(db, 10);
            }

            _body.cap.enabled = false;

            // Drop Inventory
            foreach (Inventory i in _inventory.Inventory)
                if (i.dropOnDeath)
                    _inventory.TryDrop(i, transform.position, transform.rotation, default, out _);

            // Finished off by
            if (_lastAttacker.instigator && Time.time - _lastAttacker.timeStamp < 3f)
                instigator = _lastAttacker.instigator;

            //Create Death Ticket
            DeathInfo death = new DeathInfo()
            {
                conduit = conduit,
                damageType = damageType,
                instigator = instigator,
                victim = Info
            };

            _gameModel.CharacterDied?.Invoke(death);
            Destroy(gameObject);
        }

        [ConsoleCommand("god", "Toggle god mode on players")]
        public string ToggleGodMode()
        {
            if (_isPlayer)
                if (_health != null)
                    _health = null;
                else
                    _health = new(100);

            if (_isPlayer)
                return _health != default ? "God mode disabled" : "God mode enabled";
            else
                return null;
        }
    }
}
