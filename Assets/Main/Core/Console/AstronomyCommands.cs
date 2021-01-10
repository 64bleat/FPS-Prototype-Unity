using Astronomy;
using MPConsole;
using System;

namespace MPCore
{
    [ContainsConsoleCommands]
    public static class AstronomyCommands
    {
        [ConsoleCommand("time", "set the date down to the millisecond")]
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

            TimeManager.SetCurrentTime(time);

            if (timecode == "now")
                TimeManager.SetCurrentTime(DateTime.Now);

            return $"date set to {TimeManager.currentTime}";
        }

        [ConsoleCommand("dayscale", "Scales the length of a day")]
        public static string SetDayScale(float timescale)
        {
            TimeManager.timeScale = timescale;

            return $"day scale set to {TimeManager.timeScale}.";
        }
    }
}
