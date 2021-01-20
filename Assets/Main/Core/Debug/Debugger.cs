using MPConsole;

namespace MPCore
{
    [ContainsConsoleCommands]
    public static class Debugger
    {
        public static bool enabled = false;

        [ConsoleCommand("debug", "Enter debug mode")]
        public static string ToggleDebugger()
        {
            enabled = !enabled;

            return $"Debug mode {(enabled ? "enabled" : "disabled")}";
        }
    }
}
