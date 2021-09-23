using MPConsole;
using Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = MPConsole.Console;
using Random = UnityEngine.Random;

namespace MPCore
{
	[ContainsConsoleCommands]
	public class FreeplayGameController : GameController
	{   
		PlaySettingsModel _playSettingsModel;
		protected GameModel _gameModel;
		SaveModel _saveModel;
		InputManager _input;

		CharacterInfo[] _botList;
		GameObject _currentPlayer;
		GUIModel _guiModel;

		readonly List<CharacterInfo> _loadedPlayers = new List<CharacterInfo>();
		readonly Queue<CharacterInfo> _deadBots = new Queue<CharacterInfo>();
		readonly HashSet<CharacterInfo> _liveBots = new HashSet<CharacterInfo>();
		readonly List<CharacterInfo> _orphanedBots = new List<CharacterInfo>();

		private void Awake()
		{
			_guiModel = Models.GetModel<GUIModel>();
			_playSettingsModel = Models.GetModel<PlaySettingsModel>();
			_gameModel = Models.GetModel<GameModel>();
			_saveModel = Models.GetModel<SaveModel>();
			_input = GetComponentInParent<InputManager>();

			_gameModel.GameReset.AddListener(GameReset);
			_saveModel.OnPreLoad.AddListener(_gameModel.GameReset.Invoke);
			_gameModel.GameStart.AddListener(GameStart);
			_gameModel.CharacterDied.AddListener(OnDeath);
			_gameModel.isPaused.Subscribe(SetPaused);
			Console.AddInstance(this);

			_botList = ResourceLoader.GetResources<CharacterInfo>();

			Messages.Subscribe<Character>(OnChatacterSpawned);
		}

		private void Start()
		{
			_gameModel.GameReset?.Invoke();
			_gameModel.GameStart?.Invoke();
		}

		private void OnDestroy()
		{
			_gameModel.GameReset.RemoveListener(GameReset);
			_saveModel.OnPreLoad.RemoveListener(_gameModel.GameReset.Invoke);
			_gameModel.GameStart.RemoveListener(GameStart);
			_gameModel.CharacterDied.RemoveListener(OnDeath);
			_gameModel.isPaused.Unsubscribe(SetPaused);
			Console.RemoveInstance(this);
			Models.RemoveModel<GameModel>();
			Messages.Unsubscribe<Character>(OnChatacterSpawned);
		}

		private void Update()
		{
			// Bot count may change at any time
			while (!disableBots && _loadedPlayers.Count < _playSettingsModel.botCount)
			{
				int index = _loadedPlayers.Count % _botList.Length;
				CharacterJoin(_botList[index], false);
			}

			// Attempt to spawn one bot per frame
			if (!disableBots && _deadBots.Count > 0 
				&& _liveBots.Count < _playSettingsModel.botCount)
				if (SpawnCharacter(_deadBots.Peek()))
					_deadBots.Dequeue();
		}

		void OnChatacterSpawned(Character character)
		{

		}

		void GameReset()
		{
			_loadedPlayers.Clear();
			_deadBots.Clear();
			_liveBots.Clear();
			_orphanedBots.Clear();

			Messages.Publish<GameController>(this);
		}

		void GameStart()
		{
			CharacterJoin(playerInfo, true);

			if (!disableBots)
				for (int i = 0; i < _playSettingsModel.botCount; i++)
				{
					int index = _loadedPlayers.Count % _botList.Length;
					CharacterJoin(_botList[index], false);
				}

			SpawnCharacter(_gameModel.currentPlayer.Value);
		}

		void CharacterJoin(CharacterInfo character, bool isPlayer)
		{
			character = character.Clone();

			if (isPlayer)
				_gameModel.currentPlayer.Value = character;
			else
			{
				int i = _loadedPlayers.Count;
				int repeat = i / _botList.Length;

				if(repeat > 0)
				{
					character.name = $"{character.name} ({repeat})";
					character.displayName = $"{character.displayName} ({repeat})";
				}

				_loadedPlayers.Add(character);
				_deadBots.Enqueue(character);
			}

			_gameModel.OnPlayerConnected?.Invoke(character, isPlayer);
		}

		/// <summary> Called when the game pauses or un-pauses </summary>
		void SetPaused(DeltaValue<bool> paused)
		{
			enabled = !paused.newValue;
		}

		/// <summary> called when a character dies </summary>
		private void OnDeath(DeathInfo death)
		{
			// Bot Died
			if (death.victim == _gameModel.currentPlayer.Value)
				_input.Bind("Fire", OnPlayerSpawned, this);
			else if (_playSettingsModel)
			{
				_liveBots.Remove(death.victim);

				if (_liveBots.Count + _deadBots.Count < _playSettingsModel.botCount)
					_deadBots.Enqueue(death.victim);
				else
					_orphanedBots.Add(death.victim);
			}

			// Display Death HUD Notifications
			MessageEventParameters message = default;
			message.color = Color.black;

			try
			{ 
				if (death.victim == _gameModel.currentPlayer.Value)
				{
					if (death.victim == death.instigator)
						message.message = "F";
					else
						message.message = $"You were killed by {death.instigator.displayName}";

					message.color = Color.black;
					message.bgColor = Color.white;
				}
				else if (death.instigator == _gameModel.currentPlayer.Value)
				{
					message.message = $"You killed {death.victim.displayName}";
					message.color = Color.black;
					message.bgColor = Color.white;
				}
				else
				{
					message.message = $"{death.instigator.displayName} killed {death.victim.displayName}";
					message.color = Color.grey;
					message.bgColor = new Color(0f, 0f, 0f, 0.25f);
				}
			}
			catch (Exception)
			{
				// Non-Crucial. Just too many null-checks.
			}
			finally
			{
				if (!string.IsNullOrWhiteSpace(message.message))
					_guiModel.killMessage.Value = message;
			}
		}

		/// <summary> Called when the player is ready to spawn </summary>
		private void OnPlayerSpawned()
		{
			if(SpawnCharacter(_gameModel.currentPlayer.Value))
				_input.Unbind("Fire", OnPlayerSpawned);
		}

		/// <summary> Spawn a character into the game. </summary>
		/// <param name="characterInfo"> Character to be assigned to the instantiated body </param>
		bool SpawnCharacter(CharacterInfo characterInfo)
		{
			SpawnPoint spawnPoint = SpawnPoint.GetRandomSpawnPoint();
			bool isPlayer = characterInfo == _gameModel.currentPlayer.Value;

			if (!spawnPoint)
				return false;

			// Kill if the character is aready in play
			if (_currentPlayer && isPlayer)
				if (_currentPlayer.TryGetComponent(out Character ch))
				{
					ch.Kill(ch.Info, gameObject, respawnDamageType);
					_input.Unbind("Fire", OnPlayerSpawned);
				}
				else
					Destroy(_currentPlayer);

			Character reference = (characterInfo.bodyType ? characterInfo : _gameModel.currentPlayer.Value).bodyType;
			Character instance = spawnPoint.Spawn(reference);

			SetTeam(characterInfo);
			instance.name = $"Character '{characterInfo.displayName}'";
			instance.Initialize(characterInfo, isPlayer);

			foreach (Inventory inv in spawnInventory)
				instance.Inventory.TryPickup(inv, out _);

			if (randomSpawnInventory.Count > 0)
				instance.Inventory.TryPickup(randomSpawnInventory[Random.Range(0, Mathf.Max(0, randomSpawnInventory.Count))], out _);

			if (spawnPoint.TryGetComponentInParent(out PortaSpawn spawnPs))
				spawnPs.TransferStuff(instance.Inventory);

			return true;
		}

		protected virtual void SetTeam(CharacterInfo ci)
		{
			ci.team = -1;
		}

		[ConsoleCommand("respawn", "Respawns the player")]
		public void Respawn()
		{
			SpawnCharacter(_gameModel.currentPlayer.Value);
		}

		[ConsoleCommand("player", "Selects the player GameObject in console")]
		public void TargetPlayer()
		{
			Console.target = _currentPlayer;
		}
	}
}
