using Serialization;
using System.Collections.Generic;
using System;
using UnityEngine;
using MPGUI;


namespace MPCore
{
    public class TimeTrial : MonoBehaviour
    {
        public MessageEvent messageChannel;
        public StringEvent timerChannel;
        public Transform goals;
        public RewardEvent[] rewards;

        [NonSerialized] public float timer;
        [NonSerialized] public bool ready = false;
        [NonSerialized] public bool gameOn = false;
        [NonSerialized] public float bestTime = float.MaxValue;

        private Spawner rewardSpawner;
        private ButtonSet rewardDisplay;

        private readonly HashSet<int> claimedRewards = new HashSet<int>();

        [System.Serializable]
        public class RewardEvent
        {
            public float par;
            public int goals;
            public GameObjectCount[] rewards;
        }

        [System.Serializable]
        public class GameObjectCount
        {
            public GameObject prefab;
            public int count;
        }

        private void Awake()
        {
            rewardDisplay = GetComponentInChildren<ButtonSet>();
            rewardSpawner = GetComponentInChildren<Spawner>();

            foreach(RewardEvent re in rewards)
            {
                string t = "" + re.par + "s && " + re.goals + "* => $" + re.rewards[0].count;
                rewardDisplay.AddTitle(t);
            }
        }

        private void Update()
        {
            if (gameOn)
            {
                timer += Time.deltaTime;

                if (timerChannel)
                    timerChannel.Invoke("" + timer);
            }
        }

        public void ReadyZone(bool entered)
        {
            ready = entered;

            if (entered && gameOn)
                TimeTrialEnd(false);
        }

        public void StartZoneEnter()
        {
            if (ready)
                TimeTrialStart();

        }

        public void EndZoneEnter()
        {
            if (gameOn)
                TimeTrialEnd();
        }

        private void TimeTrialStart()
        {
            gameOn = true;
            timer = 0;

            if (messageChannel)
                messageChannel.Invoke("Time Trial Start!");

            // Reset Goals
            foreach (Transform goal in goals)
                goal.gameObject.SetActive(true);
        }

        private void TimeTrialEnd(bool isValidEnd = true)
        {
            gameOn = false;

            if (isValidEnd)
            {
                bestTime = Mathf.Min(timer, bestTime);

                // Messages
                if (messageChannel)
                {
                    messageChannel.Invoke("Time Trial End! " + timer);
                    messageChannel.Invoke("BestTime: " + bestTime);
                }

                // Goals
                int goalCount = 0;
                foreach (Transform g in goals)
                    if (!g.gameObject.activeSelf)
                        goalCount++;

                // Rewards
                for (int i = 0; i < rewards.Length; i++)
                    if (timer <= rewards[i].par && goalCount >= rewards[i].goals && claimedRewards.Add(i))
                        foreach (GameObjectCount goc in rewards[i].rewards)
                            rewardSpawner.PushSpawn(goc.prefab, goc.count);
            }
            else
            {
                if (messageChannel)
                    messageChannel.Invoke("Time Trial Cancelled");
                if (timerChannel)
                    timerChannel.Invoke("");
            }
        }
    }

    [System.Serializable]
    [XMLSurrogate(typeof(TimeTrial))]
    public class TimeTrialXML : XMLSurrogate
    {
        public float timer;
        public bool ready;
        public bool gameOn;
        public float bestTime;

        public override XMLSurrogate Serialize(object o)
        {
            if(o is TimeTrial t && t)
            {
                timer = t.timer;
                ready = t.ready;
                gameOn = t.gameOn;
                bestTime = t.bestTime;
            }

            return this;
        }

        public override XMLSurrogate Deserialize(object o)
        {
            if (o is TimeTrial t && t)
            {
                t.timer = timer;
                t.ready = ready;
                t.gameOn = gameOn;
                t.bestTime = bestTime;
            }

            return this;
        }
    }
}
