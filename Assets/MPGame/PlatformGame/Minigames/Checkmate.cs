using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class Checkmate : MonoBehaviour
    {
        public List<GameObject> platforms;
        public MessageEvent messageEvent;
        public MessageEvent timer;

        private float elapsedTime;
        private float bestTime;
        private float timeCheck;
        private Character currentPlayer;
        private bool startReady = true;
        private bool gameOn = false;

        private void FixedUpdate()
        {
            elapsedTime += Time.fixedDeltaTime;

            if (elapsedTime - timeCheck > 10)
            {
                timeCheck = elapsedTime;

                messageEvent.Invoke($"{timeCheck:F0} seconds!");
            }
            timer.Invoke($"{(int)elapsedTime / 60:D2}:{(int)elapsedTime % 60:D2}:{(int)(elapsedTime % 1 * 60):D2}");
        }

        public void OnPlatformEntered(GameObject touch, GameObject platform)
        {
            if (startReady && touch.TryGetComponent(out Character character))
                GameStart(character);
        }

        public void OnEndZoneEntered(GameObject touch, GameObject platform)
        {
            // GAME END
            if (gameOn && touch.TryGetComponent(out Character character) && character == currentPlayer)
                GameOver(character);
        }

        public void OnEndZoneExit(GameObject touch, GameObject platform)
        {
            // GAME RESET
            if (!gameOn && !startReady && touch.TryGetComponent(out Character character) && character == currentPlayer)
                GameReset(character);
        }

        private void GameStart(Character character)
        {
            currentPlayer = character;
            startReady = false;
            gameOn = true;

            elapsedTime = 0;
            timeCheck = 0;

            enabled = true;

            messageEvent.Invoke("Game start!");

            character.OnDeath += GameOverOnDeath;
        }

        private void GameOver(Character character)
        {
            character.OnDeath -= GameOverOnDeath;
            gameOn = false;

            elapsedTime = Mathf.Floor(elapsedTime * 10) / 10f;
            bestTime = Mathf.Max(bestTime, elapsedTime);

            enabled = false;

            if (elapsedTime < bestTime)
                messageEvent.Invoke($"You lasted {elapsedTime}s.");
            else
                messageEvent.Invoke($"You lasted {elapsedTime}s. !!NEW PERSONAL BEST!!");
        }

        private void GameReset(Character character)
        {
            foreach (GameObject go in platforms)
                if (go.TryGetComponent(out TriggerResetPosition trp))
                    trp.ResetPosition();

            currentPlayer = null;
            startReady = true;
        }

        private void GameOverOnDeath(Character character)
        {
            GameOver(character);
            GameReset(character);
        }
    }
}