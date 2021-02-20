using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MPCore
{
    public static class PauseManager
    {
        private static readonly HashSet<Object> pauseRequests = new HashSet<Object>();
        private static event Action<bool> OnPauseUnPause;

        public static bool IsPaused { get; private set; } = false;

        public static void Push(Object pauser)
        {
            pauseRequests.Add(pauser);
            SetPause(true);
        }

        public static bool Pull(Object pauser)
        {
            pauseRequests.Remove(pauser);
            SetPause(pauseRequests.Count != 0);

            return !IsPaused;
        }

        public static void AddListener(Action<bool> onPauseUnPause)
        {
            OnPauseUnPause += onPauseUnPause;
            onPauseUnPause.Invoke(IsPaused);
        }

        public static void RemoveListener(Action<bool> onPauseUnPause)
        {
            OnPauseUnPause -= onPauseUnPause;
        }

        private static void SetPause(bool pause)
        {
            if (pause != IsPaused)
                OnPauseUnPause?.Invoke(pause);

            Cursor.lockState = pause ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = pause;

            IsPaused = pause;
        }
    }
}
