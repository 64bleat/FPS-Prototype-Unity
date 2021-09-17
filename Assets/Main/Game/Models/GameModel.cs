using MPConsole;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
	[ContainsConsoleCommands]
	public class GameModel : Models
	{
		public enum State { Ready, Playing, End }
		public DataValue<State> state = new();
		public DataValue<bool> isPaused = new();
		public DataValue<bool> debug = new();
		public DataValue<CharacterInfo> currentPlayer = new();
		public DataValue<Transform> currentView = new();
		public DataValue<int> pauseTickets = new();
		public UnityEvent GameReset = new();
		public UnityEvent GameStart = new();
		public UnityEvent GameEnd = new();
		public UnityEvent GameUnloaded = new();
		public UnityEvent<DeathInfo> CharacterDied = new();
		public UnityEvent<CharacterInfo, bool> OnPlayerConnected = new();
		public UnityEvent<CharacterInfo> SpawnCharacter = new();
		public UnityEvent<Character> OnCharacterSpawned = new();
		public UnityEvent<(CharacterInfo scorer, int score)> CharacterScored = new();

		[ConsoleCommand("debug", "Toggles debug mode")]
		static string Debug()
		{
			GameModel gameModel = GetModel<GameModel>();

			gameModel.debug.Value = !gameModel.debug.Value;

			return $"Debug mode {(gameModel.debug.Value ? "enabled" : "disabled")}.";
		}
	}
}
