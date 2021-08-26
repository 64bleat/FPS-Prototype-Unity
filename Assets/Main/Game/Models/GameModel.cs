using MPConsole;
using UnityEngine.Events;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class GameModel : Models
    {
        public enum State { Ready, Playing, End }
        public DataValue<State> state = new();
        public DataValue<bool> debug = new();
        public UnityEvent GameLoaded = new();
        public UnityEvent GameStart = new();
        public UnityEvent GameEnd = new();
        public UnityEvent GameClosed = new();
        public UnityEvent<DeathInfo> CharacterDied = new();
        public UnityEvent<CharacterInfo, bool> OnPlayerConnected = new();
        public UnityEvent<CharacterInfo> SpawnCharacter = new();
        public UnityEvent<Character> OnCharacterSpawned = new();
        public UnityEvent<(CharacterInfo scorer, int score)> CharacterScored = new();

        [ConsoleCommand("debug", "Toggles debug mode")]
        private static string Debug()
        {
            GameModel instance = GetModel<GameModel>();

            instance.debug.Value = !instance.debug.Value;

            return $"Debug mode {(instance.debug.Value ? "enabled" : "disabled")}.";
        }
    }
}
