using MPConsole;
using System;
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

		GameModel _gameModel;
		TimeModel _time;

		void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_time = Models.GetModel<TimeModel>();

			Console.AddInstance(this);

			if (_startAtRealTime || !DateTime.TryParse(_sceneStartDate, out DateTime startDate))
				startDate = DateTime.Now;

			_time.currentTime.Value = startDate;
			_time.timeScale.Value = _startingTimeScale;
			_time.dayScale.Value = _startingDayScale;
			_time.timeScale.Subscribe(SetTimeScale);
			_gameModel.isPaused.Subscribe(SetPaused);
		}

		void OnDestroy()
		{
			_gameModel.isPaused.Unsubscribe(SetPaused);
			_time.timeScale.Unsubscribe(SetTimeScale);
			Console.RemoveInstance(this);
		}

		void LateUpdate()
		{
			float elapsed = Time.deltaTime * _time.dayScale;

			_time.currentTime.Value = _time.currentTime.Value.AddSeconds(elapsed);
		}

		void SetTimeScale(DeltaValue<float> timeScale) => SetTimeScale(timeScale.newValue);
		void SetPaused(DeltaValue<bool> paused)=> SetTimeScale(paused.newValue ? 0f : _time.timeScale.Value);

		[ConsoleCommand("slomo")]
		static string Slomo(float value = 1f)
		{
			Models.GetModel<TimeModel>().timeScale.Value = value;
			return $"TimeScale set to {value:##.##}";
		}

		void SetTimeScale(float value = 1f)
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
