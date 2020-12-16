using Astronomy;
using MPConsole;
using System;

namespace MPCore
{
    [ContainsConsoleCommands]
    public static class ExtensionTest
    {
        [ConsoleCommand("time")]
        public static void SetTime(string timecode)
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


        }
    }
}
