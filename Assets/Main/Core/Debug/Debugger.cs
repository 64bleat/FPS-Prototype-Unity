using MPConsole;

namespace MPCore
{
    [ContainsConsoleCommands]
    public static class Debugger
    {
        public static bool enabled = false;

        [ConsoleCommand("debug")]
        public static void ToggleDebugger()
        {
            enabled = !enabled;
        }
    }
}
