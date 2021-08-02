using Astronomy;
using MPConsole;
using System;

namespace MPCore
{
    [ContainsConsoleCommands]
    public static class AstronomyCommands
    {
        [ConsoleCommand("time", "log the current time")]
        public static string GetTime()
        {
            return TimeManager.currentTime.ToString("s");
        }

        [ConsoleCommand("settime", "set the date down to the second")]
        public static string SetTime(DateTime time)
        {
            TimeManager.currentTime = time;

            return $"Date set to {TimeManager.currentTime}";
        }

        [ConsoleCommand("dayscale", "Scales the length of a day")]
        public static string SetDayScale(float timescale)
        {
            TimeManager.timeScale = timescale;

            return $"day scale set to {TimeManager.timeScale}.";
        }
    }
}
