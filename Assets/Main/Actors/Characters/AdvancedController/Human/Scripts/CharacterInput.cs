using UnityEngine;

namespace MPCore
{
    public class CharacterInput : MonoBehaviour
    {
        public enum MoveState { Run, Sprint, Walk, Crouch}

        public ScriptFloat toggleCrouch, toggleSprint, toggleWalk;

        private CharacterBody characterBody;
        private InputManager input;
        private MoveState moveState;
        private float stateChangeTime = 0;

        void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            input = GetComponent<InputManager>();

            input.Bind("Jump", () => jumpTimer.Reset(), this, KeyPressType.Up);
            input.Bind("Sprint", () => ToggleMove(MoveState.Sprint), this);
            input.Bind("Walk", () => ToggleMove(MoveState.Walk), this);
            input.Bind("Crouch", () => ToggleMove(MoveState.Crouch), this);
            input.Bind("Sprint", () => UntoggleMove(MoveState.Run, toggleSprint.value), this, KeyPressType.Up);
            input.Bind("Walk", () => UntoggleMove(MoveState.Run, toggleWalk.value), this, KeyPressType.Up);
            input.Bind("Crouch", () => UntoggleMove(MoveState.Run, toggleCrouch.value), this, KeyPressType.Up);

            if (TryGetComponent(out Character c))
                c.OnPlayerSet += Restart;
        }

        private void OnEnable()
        {
            Restart(false);
        }

        private void OnDestroy()
        {
            if (TryGetComponent(out Character c))
                c.OnPlayerSet -= Restart;
        }

        private void Restart(bool isPlayer)
        {
            ToggleMove(MoveState.Run);
        }

        private void ToggleMove(MoveState state)
        {
            if (input.isPlayer && input.loadKeyBindList.alwaysRun)
                if (state == MoveState.Sprint)
                    state = MoveState.Run;
                else if (state == MoveState.Run)
                    state = MoveState.Sprint;

            moveState = state;// moveState == state ? MoveState.Run : state;
            stateChangeTime = Time.time;
        }

        private void UntoggleMove(MoveState state, float time)
        {
            if (Time.time - stateChangeTime > time)
                ToggleMove(state);
        }

        public readonly System.Diagnostics.Stopwatch jumpTimer = new System.Diagnostics.Stopwatch();
        public bool Jump => input.GetKey("Jump") && (!jumpTimer.IsRunning || jumpTimer.ElapsedMilliseconds >= characterBody.defaultBunnyHopRate * 1000);
        public bool JumpHold => input.GetKey("Jump");
        public bool ProcessStep => !jumpTimer.IsRunning || jumpTimer.ElapsedMilliseconds > 250;
        public bool Glide => input.GetKey("Jump");
        public bool Sprint => moveState == MoveState.Sprint;
        public bool Walk => moveState == MoveState.Walk;
        public bool ForceCrouch { get; set; }
        public bool Crouch => ForceCrouch || moveState == MoveState.Crouch;
        public bool Crawl => characterBody.wallClimb && (!characterBody.wallClimb.onlyActivateOnCrouch || Crouch);
        public float MouseX => input.MouseX;
        public float MouseY => input.MouseY;
        public float Forward => (input.GetKey("Forward") ? 1 : 0) - (input.GetKey("Reverse") ? 1 : 0);
        public float Right => (input.GetKey("Right") ? 1 : 0) - (input.GetKey("Left") ? 1 : 0);
        public bool Interact => input.GetKey("Interact");
        public int weaponSwitchCategory;
        public bool Fire1 => input.GetKey("Fire");
        public bool Fire2 => input.GetKey("Fire");
    }
}
