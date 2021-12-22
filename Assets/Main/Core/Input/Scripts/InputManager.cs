using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MPCore
{
	public enum KeyPressType { Down, Up, Held }

	public class InputManager : MonoBehaviour
	{
		[SerializeField, FormerlySerializedAs("isPlayer")] bool _isPlayer = false;
		[SerializeField, FormerlySerializedAs("disableOnPause")] bool _disableOnPause = true;

		public UnityEvent<float> OnMouseScrollVertical = new();
		public delegate Vector2 MouseUpdateDelegate(float dt);
		public event MouseUpdateDelegate OnMouseMove;

		readonly Dictionary<string, KeyBind> _keyBinds = new Dictionary<string, KeyBind>();
		readonly Dictionary<string, List<KeyListener>> _keyListeners = new Dictionary<string, List<KeyListener>>();
		readonly List<string> _listenerKeys = new List<string>();
		readonly Dictionary<string, BotPress> _botPresses = new Dictionary<string, BotPress>();
		readonly HashSet<string> _keyMasks = new HashSet<string>();

		GameModel _gameModel;
		CanvasScaler _canvasScaler;
		KeyModel _keyModel;
		Vector2 _botMouseDelta;
		Vector2 _mousePositionDelta;
		Vector3 _lastMousePosition;

		// Buffers
		static readonly List<Action> _actionDownSet = new();
		static readonly List<Action> _actionHoldSet = new();
		static readonly List<Action> _actionUpSet = new();
		static readonly List<BotPress> _botPressEnd = new();
		static readonly List<BotPress> _botPressStart = new();
		static readonly List<KeyListener> _removeKL = new();

		private class KeyListener
		{
			public Action action;
			public Component owner;
			public KeyPressType pressType;
		}

		private class BotPress
		{
			/// <summary> What key press is being simulated </summary>
			internal string key;
			/// <summary> When the press is lifted </summary>
			internal float unpressTime;
			/// <summary> Prevents bouncing on <c>GetKeyDown</c></summary>
			internal bool started = false;
		}

		private struct BotMouse
		{
			public MouseUpdateDelegate path;
			public float unpressTime;
		}

		// UNITY
		private void Awake()
		{
			_keyModel = Models.GetModel<KeyModel>();

			_gameModel = Models.GetModel<GameModel>();
			_gameModel.isPaused.Subscribe(SetPaused);

			_canvasScaler = GetComponentInChildren<CanvasScaler>();
			_lastMousePosition = Input.mousePosition;
			LoadKeyBindList();

			if (TryGetComponent(out Character c))
				c.OnInitialized.AddListener(Initialize);
		}

		private void OnDestroy()
		{
			_keyMasks.Clear();
			_gameModel.isPaused.Unsubscribe(SetPaused);
		}

		private void LateUpdate()
		{
			foreach (BotPress bp in _botPresses.Values)
			{
				if (!bp.started)
					_botPressStart.Add(bp);

				if (bp.unpressTime < Time.time)
					_botPressEnd.Add(bp);
			}

			foreach(string key in _listenerKeys)
				if(_keyListeners.TryGetValue(key, out List<KeyListener> value))
			{
				bool isDown = GetKeyDown(key);
				bool isUp = GetKeyUp(key);
				bool isHeld = GetKey(key);

				if (isDown || isUp || isHeld)
				{ 
					foreach (KeyListener listener in value)
						if (!listener.owner)
							_removeKL.Add(listener);
						else if (listener.owner.gameObject)//.activeInHierarchy)
							if (listener.pressType == KeyPressType.Down && isDown)
								_actionDownSet.Add(listener.action);
							else if (listener.pressType == KeyPressType.Held && isHeld)
								_actionHoldSet.Add(listener.action);
							else if (listener.pressType == KeyPressType.Up && isUp)
								_actionUpSet.Add(listener.action);

					foreach (KeyListener listener in _removeKL)
						value.Remove(listener);

					_removeKL.Clear();
				}
			}

			if (_isPlayer)
			{
				_mousePositionDelta = Input.mousePosition - _lastMousePosition;

				if (_canvasScaler)
					_mousePositionDelta /= _canvasScaler.scaleFactor;

				_lastMousePosition = Input.mousePosition;
			}

			foreach (Action action in _actionDownSet)
				action?.Invoke();
			foreach (Action action in _actionHoldSet)
				action?.Invoke();
			foreach (Action action in _actionUpSet)
				action?.Invoke();
			foreach (BotPress press in _botPressStart)
				press.started = true;
			foreach (BotPress press in _botPressEnd)
				_botPresses.Remove(press.key);

			// Mosue Delta
			_botMouseDelta = Vector2.zero;
			_botMouseDelta += OnMouseMove?.Invoke(Time.deltaTime) ?? Vector2.zero;

			// Mouse Scroll
			Vector2 scroll = Input.mouseScrollDelta;

			if (scroll.y != 0)
				OnMouseScrollVertical?.Invoke(scroll.y);

			//removeBP.Clear();
			_actionDownSet.Clear();
			_actionHoldSet.Clear();
			_actionUpSet.Clear();
			_botPressEnd.Clear();
			_botPressStart.Clear();
		}

		private void Initialize(bool isPlayer)
		{
			this._isPlayer = isPlayer;
		}

		private void SetPaused(DeltaValue<bool> paused)
		{
			enabled = !_disableOnPause || !paused.newValue;
		}

		// Load Keys
		private void LoadKeyBindList()
		{
			_keyBinds.Clear();

			foreach (KeyBind kb in _keyModel.keys)
				_keyBinds.Add(kb.name, kb);
		}

		// ROBOT
		public void BotKeyDown(string key, float seconds = 0f)
		{
			if (_botPresses.TryGetValue(key, out BotPress press))
			{
				press.unpressTime = Time.time + seconds;
				press.started = false;
			}
			else
				_botPresses.Add(key, new BotPress
				{
					key = key,
					unpressTime = Time.time + seconds
				});
		}

		public void BotKeyUp(string key)
		{
			if (_botPresses.TryGetValue(key, out BotPress press))
				press.unpressTime = -1f;
		}

		// GETTING
		public bool GetKey(string keyName)
		{
			if (_keyBinds.TryGetValue(keyName, out KeyBind bn))
			{
				if (_botPresses.TryGetValue(keyName, out BotPress press))
						return true;

				foreach (KeyCode kc in bn.keyCombo)
					if (_isPlayer && Input.GetKey(kc))
						return true;
			}

			return false;
		}

		public bool GetKeyDown(string keyName)
		{
			if (_keyBinds.TryGetValue(keyName, out KeyBind bn))
			{
				if (_botPresses.TryGetValue(keyName, out BotPress press))
					if (!press.started)
						return true;

				foreach (KeyCode k in bn.keyCombo)
					if (_isPlayer && Input.GetKeyDown(k))
						return true;
			}

			return false;
		}

		public bool GetKeyUp(string keyName)
		{
			if (_keyBinds.TryGetValue(keyName, out KeyBind bn))
			{
				if (_botPresses.TryGetValue(keyName, out BotPress press))
					if (Time.time > press.unpressTime)
						return true;

				foreach (KeyCode k in bn.keyCombo)
					if (_isPlayer && Input.GetKeyUp(k))
						return true;
			}

			return false;
		}

		// BINDING
		public void Bind(string key, Action action, Component owner, KeyPressType pressType = KeyPressType.Down)
		{
			KeyListener newKeyListener = new KeyListener
			{
				action = action,
				owner = owner,
				pressType = pressType
			};

			if (!_keyListeners.TryGetValue(key, out List<KeyListener> actionList))
			{
				_keyListeners.Add(key, new List<KeyListener> { newKeyListener });
				_listenerKeys.Add(key);
			}
			else
				actionList.Add(newKeyListener);
		}

		public void Unbind(string key, Action action)
		{
			if (_keyListeners.TryGetValue(key, out List<KeyListener> listeners))
				foreach (KeyListener listener in listeners.ToArray())
					if (listener.action.Equals(action))
						listeners.Remove(listener);
		}

		public void Unbind(Component owner)
		{
			foreach (List<KeyListener> listeners in _keyListeners.Values.ToArray())
				foreach (KeyListener listener in listeners.ToArray())
					if (listener.owner.Equals(owner))
						listeners.Remove(listener);
		}

		// MOUSE
		public float MouseX => GetMouseAxis("Mouse X");
		public float MouseY => GetMouseAxis("Mouse Y");
		public Vector2 MousePositionDelta => _mousePositionDelta;

		private float GetMouseAxis(string axis)
		{
			float robotAxis = axis.Equals("Mouse X") ? _botMouseDelta.x : _botMouseDelta.y;

			return robotAxis + (!_keyMasks.Contains("Ingame") ? (_isPlayer ? Input.GetAxis(axis) : 0f) * _keyModel.mouseSensitivity : 0);
		}
	}
}
