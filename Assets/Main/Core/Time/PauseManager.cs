using UnityEngine;

namespace MPCore
{
	public class PauseManager : MonoBehaviour
	{
		GameModel _gameModel;

		void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_gameModel.pauseTickets.Subscribe(CheckPauseTickets);
			_gameModel.isPaused.Subscribe(SetPause);
		}

		void OnDestroy()
		{
			_gameModel.isPaused.Unsubscribe(SetPause);
			_gameModel.pauseTickets.Unsubscribe(CheckPauseTickets);
		}

		void CheckPauseTickets(DeltaValue<int> tickets)
		{
			bool isPaused = tickets.newValue != 0;

			if (isPaused != _gameModel.isPaused.Value)
				_gameModel.isPaused.Value = isPaused;
		}

		void SetPause(DeltaValue<bool> pause)
		{
			if (pause.newValue)
				Cursor.lockState = CursorLockMode.Confined;
			else
				Cursor.lockState = CursorLockMode.Locked;

			Cursor.visible = pause.newValue;
		}
	}
}
