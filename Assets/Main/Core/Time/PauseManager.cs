using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MPCore
{
    public class PauseManager
    {
        private static readonly HashSet<Object> pauseRequests = new HashSet<Object>();
        private static event Action<bool> OnPauseUnPause;

        public static bool IsPaused { get; private set; } = false;

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

        public static void Add(Action<bool> onPauseUnPause)
        {
            OnPauseUnPause += onPauseUnPause;
            onPauseUnPause.Invoke(IsPaused);
        }

        public static void Remove(Action<bool> onPauseUnPause)
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
