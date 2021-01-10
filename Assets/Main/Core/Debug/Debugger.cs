using MPConsole;

namespace MPCore
{
    [ContainsConsoleCommands]
    public static class Debugger
    {
        public static bool enabled = false;

        [ConsoleCommand("debug", "Enter debug mode")]
        public static void ToggleDebugger()
        {
            enabled = !enabled;
        }
    }
}
