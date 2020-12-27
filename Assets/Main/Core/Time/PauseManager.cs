using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class PauseManager
    {
        public delegate void Paused(bool paused);

        public static bool IsPaused { get; private set; } = false;

        private static readonly HashSet<Object> pauseRequests = new HashSet<Object>();
        private static event Paused OnPauseUnPause;

        public static void Reset()
        {
            IsPaused = false;
            pauseRequests.Clear();
            OnPauseUnPause = null;
        }

        public static void Request(Object pauser)
        {
            pauseRequests.Add(pauser);
            SetPause(true);
        }

        public static bool Release(Object pauser)
        {
            pauseRequests.Remove(pauser);
            SetPause(pauseRequests.Count != 0);

            return !IsPaused;
        }

        public static void Add(Paused onPauseUnPause)
        {
            OnPauseUnPause += onPauseUnPause;
            onPauseUnPause.Invoke(IsPaused);
        }

        public static void Remove(Paused onPauseUnPause)
        {
            OnPauseUnPause -= onPauseUnPause;
        }

        private static void SetPause(bool pause)
        {
            if (pause != IsPaused)
                OnPauseUnPause?.Invoke(pause);

            IsPaused = pause;
        }
    }
}
