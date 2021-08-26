using MPConsole;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Console = MPConsole.Console;

namespace MPCore
{
    [ContainsConsoleCommands]
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] string _sceneStartDate;
        [SerializeField] bool _startAtRealTime = true;
        [SerializeField] float _startingTimeScale = 1;
        [SerializeField] float _startingDayScale = 1;

        TimeModel _time;

        private void Awake()
        {
            Console.AddInstance(this);

            // Time Model
            _time = Models.GetModel<TimeModel>();

            _time.timeScale.OnSet.AddListener(OnSetTimeScale);
            PauseManager.AddListener(OnPauseUnPause);

            if (_startAtRealTime || !DateTime.TryParse(_sceneStartDate, out DateTime startDate))
                startDate = DateTime.Now;

            _time.currentTime.Value = startDate;
            _time.timeScale.Value = _startingTimeScale;
            _time.dayScale.Value = _startingDayScale;
        }

        private void OnDestroy()
        {
            _time.timeScale.OnSet.RemoveListener(OnSetTimeScale);
            Console.RemoveInstance(this);
            PauseManager.RemoveListener(OnPauseUnPause);
        }

        private void LateUpdate()
        {
            float elapsed = Time.deltaTime * _time.dayScale;

            _time.currentTime.Value = _time.currentTime.Value.AddSeconds(elapsed);
        }

        private void OnSetTimeScale(DeltaValue<float> timeScale)
        {
            Time.timeScale = timeScale.newValue;
            Time.fixedDeltaTime = 1f / 120f * Mathf.Min(1, timeScale.newValue);
        }

        private void OnPauseUnPause(bool paused)
        {
            SetTimeScale(paused ? 0f : _time.timeScale);
        }

        [ConsoleCommand("slomo")]
        private void SetTimeScale(float value = 1f)
        {
            Time.timeScale = value;
            Time.fixedDeltaTime = 1f / 120f * Mathf.Min(1, value);
        }

        [ConsoleCommand("time", "log the current time")]
        string GetTime()
        {
            return _time.currentTime.Value.ToString("s");
        }

        [ConsoleCommand("settime", "set the date down to the second")]
        string SetTime(DateTime time)
        {
            _time.currentTime.Value = time;

            return $"Date set to {time}";
        }

        [ConsoleCommand("dayscale", "Scales the length of a day")]
        string SetDayScale(float timeScale = 1f)
        {
            _time.dayScale.Value = timeScale;

            return $"day scale set to {timeScale}.";
        }
    }
}
