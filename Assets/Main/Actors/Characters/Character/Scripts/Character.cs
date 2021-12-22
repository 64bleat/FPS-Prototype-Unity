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
		public string displayName;
		[SerializeReference] ResourceValue _health = new(100);

		public readonly UnityEvent<bool> OnInitialized = new();

		AIModel _aiModel;
		GUIModel _guiModel;
		GameModel _gameModel;
		CharacterBody _body;
		InventoryManager _inventory;
		DamageEvent _damage;
		WeaponSwitcher _weapons;
		DamageTicket _lastAttacker;
		bool _isPlayer;
		CharacterInfo _characterInfo;

		bool _invulderable;

		public CharacterBody Body => _body;
		public CharacterInfo Info => _characterInfo;
		public InventoryManager Inventory => _inventory;
		public WeaponSwitcher Weapons => _weapons;
		public bool IsPlayer => _isPlayer;
		public ResourceValue Health => _health;

		void Awake()
		{
			_aiModel = Models.GetModel<AIModel>();
			_guiModel = Models.GetModel<GUIModel>();
			_gameModel = Models.GetModel<GameModel>();
			_inventory = GetComponent<InventoryManager>();
			_body = GetComponent<CharacterBody>();
			_damage = GetComponent<DamageEvent>();
			_weapons = GetComponent<WeaponSwitcher>();
			_damage.OnHit += Damage;

			// Health Listener Non-Initialized
			_health.Subscribe(health =>
			{
				// Display player health
				if (_isPlayer)
					_guiModel.health.Value = health.newValue;

				// Kill  when health depleats
				if (health.oldValue > 0 && health.newValue <= 0)
					Kill(_lastAttacker.instigator, _lastAttacker.instigatorBody, _lastAttacker.damageType);
			}, false);
		}

		void OnEnable()
		{
			_aiModel.characters.Add(this);
			AIBlackboard.visualTargets.Add(this);
			Console.AddInstance(this);
		}

		void OnDisable()
		{
			_aiModel.characters.Remove(this);
			AIBlackboard.visualTargets.Remove(this);
			Console.RemoveInstance(this);            
		}

		public void Initialize(CharacterInfo info, bool isPlayer)
		{
			_characterInfo = info;
			_isPlayer = isPlayer;
			_health.Value = _health.Value;
			OnInitialized?.Invoke(_isPlayer);
			MessageBus.Publish(this);
		}

		void Damage(DamageTicket ticket)
		{
			_lastAttacker = ticket;
			_health.Value -= ticket.deltaValue;
			_body.Sound.PlayHurtSound(ticket.deltaValue);
		}

		public DeltaValue<int> Heal(int value, GameObject owner, CharacterInfo instigator, GameObject conduit)
		{
			int oldValue = _health.Value;
			_health.Value += value;
			return new DeltaValue<int>(oldValue, _health.Value);
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

			_body.HitBox.enabled = false;

			// Drop Inventory
			foreach (Inventory i in _inventory.Inventory)
				if (i.dropOnDeath)
					_inventory.TryDrop(i, transform.position, transform.rotation, default, out _);

			// Finished off by
			if (_lastAttacker.instigator && Time.time - _lastAttacker.timeStamp < 3f)
				instigator = _lastAttacker.instigator;

			MessageBus.Publish<GameModel.CharacterDied>(new()
			{
				conduit = conduit,
				damageType = damageType,
				instigator = instigator,
				victim = Info
			});

			Destroy(gameObject);
		}

		[ConsoleCommand("god", "Toggle god mode on players")]
		string ToggleGodMode()
		{
			if (_isPlayer)
			{
				_invulderable = !_invulderable;

				if (_invulderable)
					_damage.OnHit -= Damage;
				else
					_damage.OnHit += Damage;
			}

			if (_isPlayer)
				return _invulderable ? "God mode enabled." : "God mode disabled.";
			else
				return null;
		}
	}
}
