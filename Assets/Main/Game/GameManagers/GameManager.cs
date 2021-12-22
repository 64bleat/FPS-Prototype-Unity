using MPConsole;
using MPCore;
using Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterInfo = MPCore.CharacterInfo;
using Console = MPConsole.Console;

namespace MPGame
{
	[ContainsConsoleCommands]
	public class GameManager : MonoBehaviour
	{
		[SerializeField] protected List<Inventory> _spawnInventory;
		[SerializeField] protected CharacterInfo _playerInfo;
		[SerializeField] protected DamageType _respawnDamageType;
		[SerializeField] protected bool _disableBots = false;

		protected PlaySettingsModel _playSettingsModel;
		protected GameModel _gameModel;
		protected SaveModel _saveModel;
		protected InputManager _input;
		protected CharacterInfo[] _botList;
		protected Character _currentPlayer;
		protected GUIModel _guiModel;

		protected readonly HashSet<CharacterInfo> _loadedCharacters = new HashSet<CharacterInfo>();

		public CharacterInfo PlayerInfo => _playerInfo;
		public List<Inventory> SpawnInventory => _spawnInventory;

		void Awake()
		{
			_guiModel = Models.GetModel<GUIModel>();
			_playSettingsModel = Models.GetModel<PlaySettingsModel>();
			_gameModel = Models.GetModel<GameModel>();
			_saveModel = Models.GetModel<SaveModel>();
			_input = GetComponentInParent<InputManager>();

			MessageBus.Subscribe<GameModel.GameReset>(GameReset);
			MessageBus.Subscribe<GameModel.GameStart>(GameStart);
			MessageBus.Subscribe<GameModel.CharacterJoined>(CharacterJoined);
			MessageBus.Subscribe<GameModel.CharacterDied>(CharacterDied);
			MessageBus.Subscribe<SaveModel.PreLoad>(PublishGameReset);
			_gameModel.isPaused.Subscribe(SetPaused);
			Console.AddInstance(this);

			_botList = ResourceLoader.GetResources<CharacterInfo>();
		}

		void Start()
		{
			MessageBus.Publish<GameManager>(this);
			MessageBus.Publish<GameModel.GameReset>();
			MessageBus.Publish<GameModel.GameStart>();
		}

		void OnDestroy()
		{
			_gameModel.isPaused.Unsubscribe(SetPaused);

			MessageBus.Unsubscribe<GameModel.GameReset>(GameReset);
			MessageBus.Unsubscribe<GameModel.GameStart>(GameStart);
			MessageBus.Unsubscribe<GameModel.CharacterJoined>(CharacterJoined);
			MessageBus.Unsubscribe<GameModel.CharacterDied>(CharacterDied);
			MessageBus.Unsubscribe<SaveModel.PreLoad>(PublishGameReset);

			Console.RemoveInstance(this);
			Models.RemoveModel<GameModel>();
		}

		void PublishGameReset(SaveModel.PreLoad _) => MessageBus.Publish<GameModel.GameReset>();

		void GameReset(GameModel.GameReset _)
		{
			StopAllCoroutines();
			_loadedCharacters.Clear();
		}

		void GameStart(GameModel.GameStart _)
		{
			MessageBus.Publish(new GameModel.CharacterJoined(_playerInfo.Clone(), true));
			StartCoroutine(RespawnPlayer(0f, true));

			if (!_disableBots)
				for (int i = 0; i < _playSettingsModel.botCount; i++)
				{
					int index = _loadedCharacters.Count % _botList.Length;
					MessageBus.Publish(new GameModel.CharacterJoined(_botList[index].Clone(), false));
				}
		}

		void CharacterJoined(GameModel.CharacterJoined join)
		{
			if (join.isPlayer)
				_gameModel.currentPlayer.Value = join.character;
			else
			{
				int i = _loadedCharacters.Count;
				int repeat = i / _botList.Length;

				if (repeat > 0)
				{
					join.character.name = $"{join.character.name} ({repeat})";
					join.character.displayName = $"{join.character.displayName} ({repeat})";
				}

				StartCoroutine(RespawnBot(join.character, 0f));
			}

			_loadedCharacters.Add(join.character);
		}

		/// <summary> Called when the game pauses or un-pauses </summary>
		void SetPaused(DeltaValue<bool> paused)
		{
			enabled = !paused.newValue;
		}

		IEnumerator RespawnBot(CharacterInfo character, float seconds)
		{
			yield return new WaitForSeconds(seconds);
			yield return new WaitUntil(() => SpawnCharacter(character));
		}

		IEnumerator RespawnPlayer(float seconds, bool autoSpawn)
		{
			yield return new WaitForSeconds(seconds);

			if (autoSpawn)
				yield return new WaitUntil(() => SpawnCharacter(_gameModel.currentPlayer.Value));
			else
				_input.Bind("Fire", RespawnPlayerOnClick, this);
		}

		void RespawnPlayerOnClick()
		{
			if (SpawnCharacter(_gameModel.currentPlayer.Value))
				_input.Unbind("Fire", RespawnPlayerOnClick);
		}

		void CharacterDied(GameModel.CharacterDied death)
		{
			if (death.victim == _gameModel.currentPlayer.Value)
				StartCoroutine(RespawnPlayer(1f, false));
			else if (_playSettingsModel)
				StartCoroutine(RespawnBot(death.victim, 5f));

			// Display Death HUD Notifications
			bool victimIsPlayer = death.victim == _gameModel.currentPlayer.Value;
			bool instigatorIsPlayer = death.instigator == _gameModel.currentPlayer.Value;
			switch((victimIsPlayer, instigatorIsPlayer))
			{
				case (true, true):
					_guiModel.killMessage.Value = new MessageEventParameters()
					{
						message = "F",
						color = Color.red,
						bgColor = Color.white
					};
					break;
				case (true, false):
					if (death.instigator)
						_guiModel.killMessage.Value = new MessageEventParameters()
						{
							message = $"{death.instigator.displayName} killed {death.victim.displayName}",
							color = Color.red,
							bgColor = Color.white
						};
					else
						_guiModel.killMessage.Value = new MessageEventParameters()
						{
							message = $"{death.victim.displayName} died",
							color = Color.red,
							bgColor = Color.white
						};
					break;
				case (false, true):
					if (death.victim)
						_guiModel.killMessage.Value = new MessageEventParameters()
						{
							message = $"{death.instigator.displayName} killed {death.victim.displayName}",
							color = Color.black,
							bgColor = Color.white
						};
					break;
				case (false, false):
					if (death.victim)
						if(death.instigator)
							_guiModel.killMessage.Value = new MessageEventParameters()
							{
								message = $"{death.instigator.displayName} killed {death.victim.displayName}",
								color = Color.grey,
								bgColor = new Color(0f, 0f, 0f, 0.25f)
							};
						else
							_guiModel.killMessage.Value = new MessageEventParameters()
							{
								message = $"{death.victim.displayName} died",
								color = Color.grey,
								bgColor = new Color(0f, 0f, 0f, 0.25f)
							};
					break;
			}
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
			{
				_currentPlayer.Kill(_currentPlayer.Info, gameObject, _respawnDamageType);
				_input.Unbind("Fire", RespawnPlayerOnClick);
			}

			Character reference = (characterInfo.bodyType ? characterInfo : _gameModel.currentPlayer.Value).bodyType;
			Character instance = spawnPoint.Spawn(reference);

			SetTeam(characterInfo);
			instance.name = $"Character '{characterInfo.displayName}'";
			instance.Initialize(characterInfo, isPlayer);

			foreach (Inventory inv in _spawnInventory)
				instance.Inventory.TryPickup(inv, out _);

			if (spawnPoint.TryGetComponentInParent(out PortaSpawn spawnPs))
				spawnPs.TransferStuff(instance.Inventory);

			if(isPlayer)
				_currentPlayer = instance;

			return true;
		}

		protected virtual void SetTeam(CharacterInfo ci)
		{
			ci.team = -1;
		}

		[ConsoleCommand("respawn", "Respawns the player")]
		void Respawn()
		{
			SpawnCharacter(_gameModel.currentPlayer.Value);
		}

		[ConsoleCommand("player", "Selects the player GameObject in console")]
		void TargetPlayer()
		{
			Console.target = _currentPlayer;
		}
	}
}
