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
        public static string SetTime(string timecode)
        {
            DateTime time = TimeManager.currentTime;

            switch(timecode)
            {
                case "now":
                    time = DateTime.Now;
                    break;
                case "utcnow":
                    time = DateTime.UtcNow;
                    break;
                default:
                    if(DateTime.TryParse(timecode, out DateTime result))
                        time = result;
                    break;
            }

            TimeManager.currentTime = time;

            if (timecode == "now")
                TimeManager.currentTime = DateTime.Now;

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
