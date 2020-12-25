using UnityEngine;

namespace MPCore.MPCore.Checkmate
{
    public class Checkmate : MiniGame
    {
        public TriggerResetPosition[] resetObjects = new TriggerResetPosition[0];
        public MessageEvent onShortMessageSend;

        private float timer;
        private float bestTime;
        private float timeCheck;

        private void Awake()
        {
            State.Add(new State("GameReady"),
                new State("GameOn", start: GameOnStart, fixedUpdate: GameOnFixedUpdate, end: GameOnEnd),
                new State("GameOff", fixedUpdate: CheckForPlayer));
            State.Initialize("GameReady");
        }

        public override bool GameStart(GameObject player)
        {
            if (State.IsCurrentState("GameReady") && player && player.layer == LayerMask.NameToLayer("Player"))
            {
                Player = player;
                State.SwitchTo("GameOn", true);

                return true;
            }

            return false;
        }

        public override bool GameEnd(GameObject player)
        {
            if (State.IsCurrentState("GameOn") && Player == player)
            {
                State.SwitchTo("GameOff", true);
                return true;
            }

            return false;
        }

        public override bool GameReset(GameObject player)
        {
            if (Player == player && State.IsCurrentState("GameOff"))
            {
                State.SwitchTo("GameReady");

                //reset objects
                if (resetObjects != null)
                    foreach (TriggerResetPosition trp in resetObjects)
                        if (trp)
                            trp.ResetPosition();

                return true;
            }

            return false;
        }

        // STATE GAMEON ===========================================================
        private void GameOnStart()
        {
            timer = 0;
            timeCheck = 0;

            if (onShortMessageSend)
                onShortMessageSend.Invoke("Game start!");
        }

        private void GameOnFixedUpdate()
        {
            if (Player)
            {
                timer += Time.fixedDeltaTime;
                if (timer - timeCheck > 10)
                {
                    timeCheck = Mathf.Round(timer);

                    if (onShortMessageSend)
                        onShortMessageSend.Invoke("" + timeCheck + " seconds!");

                    //BroadcastSystem.Broadcast("HUD", "ShortMessage", "" + timeCheck + " seconds!");
                }
            }
            else
                State.SwitchTo("GameReady");
        }

        private void GameOnEnd()
        {
            timer = Mathf.Floor(timer * 10) / 10f;
            bestTime = Mathf.Max(bestTime, timer);

            if (onShortMessageSend)
            {
                onShortMessageSend.Invoke("You lasted " + timer + "s.");

                if (timer < bestTime)
                    onShortMessageSend.Invoke("You lasted " + timer + "s.");
                else
                    onShortMessageSend.Invoke("!!NEW PERSONAL BEST!!");

                //BroadcastSystem.Broadcast("HUD", "ShortMessage", "You lasted " + timer + "s.");

                //if (timer < bestTime)
                //    BroadcastSystem.Broadcast("HUD", "ShortMessage", "You lasted " + timer + "s.");
                //else
                //    BroadcastSystem.Broadcast("HUD", "ShortMessage", "!!NEW PERSONAL BEST!!");
            }
        }

        private void CheckForPlayer()
        {
            if (!Player)
                State.SwitchTo("GameReady");
        }
    }
}