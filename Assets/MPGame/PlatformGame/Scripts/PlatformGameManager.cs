using System.Collections;
using UnityEngine;

namespace MPCore
{
	public class PlatformGameManager : MonoBehaviour
	{
		PlatformGameModel _minigameModel;
		GUIModel _guiModel;
		GameModel _gameModel;
		Coroutine _timerCoroutine;

		void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_guiModel = Models.GetModel<GUIModel>();
			_minigameModel = Models.GetModel<PlatformGameModel>();
			_minigameModel.bestTime.Subscribe(MessageNewBestTime);
			_minigameModel.OnStart.AddListener(GameStart);
			_minigameModel.OnEnd.AddListener(GameEnd);
			_minigameModel.OnReset.AddListener(GameReset);
			_minigameModel.gameState.Value = PlatformGameModel.State.Reset;
			_minigameModel.isReset.Value = true;
		}

		void OnDestroy()
		{
			Models.RemoveModel<PlatformGameModel>();
		}

		IEnumerator StartGameTimer(float interval)
		{
			float startTime = Time.time;

			while(true)
			{
				float currentTime = Time.time - startTime;
				float lastFrameTime = currentTime - Time.deltaTime;

				if (lastFrameTime % interval > currentTime % interval)
					_guiModel.shortMessage.Value = $"{currentTime:F0} seconds!";

				_minigameModel.elapsedTime.Value = currentTime;

				yield return null;
			}
		}

		void GameStart(Character character)
		{
			_minigameModel.gameState.Value = PlatformGameModel.State.Playing;
			_minigameModel.currentPlayer.Value = character.Info;
			_minigameModel.isReset.Value = false;
			_minigameModel.elapsedTime.Value = 0;
			_guiModel.shortMessage.Value = "Game start!";
			_timerCoroutine = StartCoroutine(StartGameTimer(10f));

			//_gameModel.CharacterDiedOld.AddListener(CharacterDiedGameEnd);
			MessageBus.Subscribe<GameModel.CharacterDied>(CharacterDiedGameEnd);
		}

		void GameEnd()
		{
			float elapsedTime = _minigameModel.elapsedTime;
			float best = _minigameModel.bestTime.Value.time;

			_minigameModel.gameState.Value = PlatformGameModel.State.Stopped;
			//_gameModel.CharacterDiedOld.RemoveListener(CharacterDiedGameEnd);
			MessageBus.Unsubscribe<GameModel.CharacterDied>(CharacterDiedGameEnd);

			if (elapsedTime > best)
				_minigameModel.bestTime.Value = new PlatformGameModel.TimeRecord()
				{
					time = elapsedTime,
					holder = _minigameModel.currentPlayer.Value
				};
			else
				_guiModel.shortMessage.Value = $"You lasted {elapsedTime:F2}s.";

			StopCoroutine(_timerCoroutine);
			_timerCoroutine = null;
		}

		void CharacterDiedGameEnd(GameModel.CharacterDied death)
		{
			if(death.victim == _minigameModel.currentPlayer.Value)
			{
				_minigameModel.OnEnd?.Invoke();
				_minigameModel.OnReset?.Invoke();
			}
		}

		void GameReset()
		{
			_minigameModel.gameState.Value = PlatformGameModel.State.Reset;
			_minigameModel.currentPlayer.Value = null;
			_minigameModel.isReset.Value = true;
		}

		void MessageNewBestTime(DeltaValue<PlatformGameModel.TimeRecord> record)
		{
			float time = record.newValue.time;
			float oldTime = record.oldValue.time;

			if (time > oldTime)
			{
				string oldName;
				string message;

				if (record.oldValue.holder)
				{
					if (record.newValue.holder == record.oldValue.holder)
						oldName = "your own";
					else
						oldName = $"{record.oldValue.holder.displayName}'s";

					message = $"You beat {oldName} score with {time:F2} seconds!";
				}
				else
				{
					message = $"You set a new record of {time:F2} seconds!";
				}

				_guiModel.shortMessage.Value = message;
			}
		}
	}
}