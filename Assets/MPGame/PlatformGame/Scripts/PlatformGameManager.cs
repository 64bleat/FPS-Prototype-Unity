using System.Collections;
using UnityEngine;

namespace MPCore
{
    public class PlatformGameManager : MonoBehaviour
    {
        [SerializeField] private PlatformGameModel _gameModel;
        [SerializeField] private GUIModel _guiModel;

        private Coroutine _timerCoroutine;

        private void Awake()
        {
            _gameModel = Models.GetModel<PlatformGameModel>();
            _gameModel.bestTime.OnSet.AddListener(MessageNewBestTime);
            _gameModel.OnStart.AddListener(GameStart);
            _gameModel.OnEnd.AddListener(GameEnd);
            _gameModel.OnReset.AddListener(GameReset);
            _gameModel.gameState.Value = PlatformGameModel.State.Reset;
            _gameModel.isReset.Value = true;

            _guiModel = Models.GetModel<GUIModel>();
        }

        private void OnDestroy()
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
                    _guiModel.ShortMessage.Invoke($"{currentTime:F0} seconds!");

                _gameModel.elapsedTime.Value = currentTime;

                yield return null;
            }
        }

        private void GameStart(Character character)
        {
            _gameModel.gameState.Value = PlatformGameModel.State.Playing;
            _gameModel.currentPlayer.Value = character;
            _gameModel.isReset.Value = false;
            _gameModel.elapsedTime.Value = 0;
            _guiModel.ShortMessage.Invoke("Game start!");
            _timerCoroutine = StartCoroutine(StartGameTimer(10f));

            character.OnDeath += GameEndOnDeath;
        }

        private void GameEnd()
        {
            _gameModel.gameState.Value = PlatformGameModel.State.Stopped;
            Character character = _gameModel.currentPlayer;
            float elapsedTime = _gameModel.elapsedTime;
            character.OnDeath -= GameEndOnDeath;

            float best = _gameModel.bestTime.Value.time;

            if (elapsedTime > best)
                _gameModel.bestTime.Value = new PlatformGameModel.TimeRecord()
                {
                    time = elapsedTime,
                    holder = character.characterInfo
                };
            else
                _guiModel.ShortMessage.Invoke($"You lasted {elapsedTime:F2}s.");

            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        private void GameEndOnDeath(Character character)
        {
            _gameModel.OnEnd?.Invoke();
            _gameModel.OnReset?.Invoke();
        }

        public void GameReset()
        {
            _gameModel.gameState.Value = PlatformGameModel.State.Reset;
            _gameModel.currentPlayer.Value = null;
            _gameModel.isReset.Value = true;
        }

        private void MessageNewBestTime(DeltaValue<PlatformGameModel.TimeRecord> record)
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

                _guiModel.ShortMessage.Invoke(message);
            }
        }
    }
}