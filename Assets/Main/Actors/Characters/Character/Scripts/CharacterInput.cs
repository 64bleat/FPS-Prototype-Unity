using UnityEngine;

namespace MPCore
{
	public class CharacterInput : MonoBehaviour
	{
		public enum MoveState { Run, Sprint, Walk, Crouch }
		public DataValue<MoveState> moveState = new(MoveState.Run);

		KeyModel _keyModel;
		CharacterBody _characterBody;
		InputManager _input;
		Character _character;
		float _stateChangeTime = 0;

		void Awake()
		{
			_keyModel = Models.GetModel<KeyModel>();
			_characterBody = GetComponent<CharacterBody>();
			_input = GetComponent<InputManager>();
			_character = GetComponent<Character>();

			_input.Bind("Jump", () => jumpTimer.Reset(), this, KeyPressType.Up);
			_input.Bind("Sprint", () => ToggleMove(MoveState.Sprint), this);
			_input.Bind("Walk", () => ToggleMove(MoveState.Walk), this);
			_input.Bind("Crouch", () => ToggleMove(MoveState.Crouch), this);
			_input.Bind("Sprint", () => UntoggleMove(MoveState.Run, _keyModel.sprintToggleTime), this, KeyPressType.Up);
			_input.Bind("Walk", () => UntoggleMove(MoveState.Run, _keyModel.walkToggleTime), this, KeyPressType.Up);
			_input.Bind("Crouch", () => UntoggleMove(MoveState.Run, _keyModel.crouchToggleTime), this, KeyPressType.Up);
			_character.OnInitialized.AddListener(Initialize);
		}

		void OnEnable()
		{
			Initialize(false);
		}

		void Initialize(bool isPlayer)
		{
			ToggleMove(MoveState.Run);
		}

		void ToggleMove(MoveState state)
		{
			if (_character.IsPlayer && _keyModel.alwaysRun)
				if (state == MoveState.Sprint)
					state = MoveState.Run;
				else if (state == MoveState.Run)
					state = MoveState.Sprint;

			moveState.Value = state;// moveState == state ? MoveState.Run : state;
			_stateChangeTime = Time.time;
		}

		void UntoggleMove(MoveState state, float time)
		{
			if (Time.time - _stateChangeTime > time)
				ToggleMove(state);
		}

		public readonly System.Diagnostics.Stopwatch jumpTimer = new System.Diagnostics.Stopwatch();
		public bool Jump => _input.GetKey("Jump") && (!jumpTimer.IsRunning || jumpTimer.ElapsedMilliseconds >= _characterBody.defaultBunnyHopRate * 1000);
		//public bool JumpHold => _input.GetKey("Jump");
		public bool ProcessStep => !jumpTimer.IsRunning || jumpTimer.ElapsedMilliseconds > 250;
		public bool Glide => _input.GetKey("Jump");
		public bool Sprint => moveState.Value == MoveState.Sprint;
		public bool Walk => moveState.Value == MoveState.Walk;
		public bool ForceCrouch { get; set; }
		public bool Crouch => ForceCrouch || moveState.Value == MoveState.Crouch;
		public bool Crawl => _characterBody.wallClimb && (!_characterBody.wallClimb.onlyActivateOnCrouch || Crouch);
		public float MouseX => _input.MouseX;
		public float MouseY => _input.MouseY;
		public float Forward => (_input.GetKey("Forward") ? 1 : 0) - (_input.GetKey("Reverse") ? 1 : 0);
		public float Right => (_input.GetKey("Right") ? 1 : 0) - (_input.GetKey("Left") ? 1 : 0);
		//public bool Interact => _input.GetKey("Interact");
		//public int weaponSwitchCategory;
		//public bool Fire1 => _input.GetKey("Fire");
		//public bool Fire2 => _input.GetKey("Fire");
	}
}
