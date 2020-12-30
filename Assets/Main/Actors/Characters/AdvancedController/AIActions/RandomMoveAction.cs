using System.Diagnostics;
using UnityEngine;

namespace MPCore
{
    public class RandomMoveAction : AIAction
    {
        private InputManager input;
        private readonly Stopwatch timer = new Stopwatch();
        private float nextTime = 1f;

        private readonly string[] buttons = new string[]
        {
            "Forward", "Reverse", "Left", "Right",
            "Jump", "Crouch", "Sprint", "Walk"
        };

        public override void InstanceAwake()
        {
            isFinal = true;
            input = gameObject.GetComponent<InputManager>();
        }

        public override float? Priority(IGOAPAction successor)
        {
            return 0;
        }

        public override void OnStart()
        {
            timer.Start();
        }

        public override GOAPStatus Update()
        {
            if (input && timer.Elapsed.TotalSeconds > nextTime)
            {
                Vector2 randomDir = new Vector2(Random.value, Random.value).normalized * Random.Range(-270f, 270f);
                nextTime = Random.Range(0.125f, 4f);
                input.Press(buttons[Random.Range(0, buttons.Length)], Random.Range(0, 0.25f));

                input.MouseMove(t => randomDir * Time.deltaTime, Random.Range(0.125f, 0.75f));
                timer.Restart();
            }

            return input ? GOAPStatus.Running : GOAPStatus.Fail;
        }
    }
}
