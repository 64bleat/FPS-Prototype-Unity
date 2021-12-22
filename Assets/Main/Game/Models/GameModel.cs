using MPConsole;
using UnityEngine;

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

		public struct GameReset { }
		public struct GameStart { }
		public struct CharacterDied
		{
			public GameObject conduit;
			public CharacterInfo instigator;
			public CharacterInfo victim;
			public DamageType damageType;
		}
		public struct CharacterJoined
		{
			public readonly CharacterInfo character;
			public readonly bool isPlayer;

			public CharacterJoined(CharacterInfo character, bool isPlayer)
			{
				this.character = character;
				this.isPlayer = isPlayer;
			}
		}

		[ConsoleCommand("debug", "Toggles debug mode")]
		static string Debug()
		{
			GameModel gameModel = GetModel<GameModel>();

			gameModel.debug.Value = !gameModel.debug.Value;

			return $"Debug mode {(gameModel.debug.Value ? "enabled" : "disabled")}.";
		}
	}
}
