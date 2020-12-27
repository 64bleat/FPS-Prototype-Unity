using MPConsole;
using UnityEngine;

namespace MPCore
{
    [ContainsConsoleCommands]
    public static class GameTime
    {
        public const int MaxTimeScale = 100;
        private static float currentTimeScale = 1f;
        private static bool isPaused = false;

        [ConsoleCommand("slomo")]
        public static void SetTimeScale(float ts)
        {
            ts = Mathf.Clamp(ts, 0, MaxTimeScale);

            currentTimeScale = ts;

            if (!isPaused)
                SetTime(ts);
        }

        public static void InitTime()
        {
            SetTime(currentTimeScale);
        }

        private static void SetTime(float ts)
        {
            Time.timeScale = ts;
            Time.fixedDeltaTime = 1f / 120f * Mathf.Min(1, ts);
        }

        public static void OnPauseUnPause(bool paused)
        {
            isPaused = paused;

            if (paused)
                SetTime(0f);
            else
                SetTime(currentTimeScale);
        }
    }
}
