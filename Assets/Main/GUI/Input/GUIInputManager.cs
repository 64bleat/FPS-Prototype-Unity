using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MPGUI
{
	/// <summary>
	/// ROOT COMPONENT:
	///     This controls all input related to GUI.
	/// </summary>
	public class GUIInputManager : MonoBehaviour
	{
		static readonly KeyCode[] MOUSE_KEYS = {
			KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2,
			KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5};
		static readonly KeyCode[] MODIFIERS = {
			KeyCode.LeftShift, KeyCode.RightShift,
			KeyCode.LeftAlt, KeyCode.RightAlt,
			KeyCode.LeftControl, KeyCode.RightControl };
		public static readonly Dictionary<KeyCode, uint> BITMASK = new() {
			{KeyCode.Mouse0, 1 << 0 },
			{KeyCode.Mouse1, 1 << 1 },
			{KeyCode.Mouse2, 1 << 2 },
			{KeyCode.Mouse3, 1 << 3 },
			{KeyCode.Mouse4, 1 << 4 },
			{KeyCode.Mouse5, 1 << 5 },
			{KeyCode.LeftShift, 1 << 6 },
			{KeyCode.RightShift, 1 << 6},
			{KeyCode.LeftAlt, 1 << 7 },
			{KeyCode.RightAlt, 1 << 7 },
			{KeyCode.LeftControl, 1 << 8 },
			{KeyCode.RightControl, 1 << 8 }};

		static readonly List<RaycastResult> _mouseCastList = new();
		static readonly HashSet<IGUISelectable> _selectionAddHash = new();
		static readonly List<IGUISelectable> _selectionAddList = new();
		static readonly List<IClickable> _clickableBuffer = new(50);
		static readonly MouseInfo _mRaw = new();
		static PointerEventData _pointer;

		readonly List<IGUISelectable> _selectionCurrentList = new();
		readonly HashSet<IGUISelectable> _selectionCurrentHash = new();
		readonly MouseInfo _mDown = new();

		void OnEnable()
		{
			_pointer = new PointerEventData(EventSystem.current);
		}

		void LateUpdate()
		{
			GetMouseInfo(_mRaw);

			uint downKeyMask = 0;
			uint holdKeyMask = 0;

			// Get mouse buttons down
			foreach(KeyCode mouseKey in MOUSE_KEYS)
				if(Input.GetKeyDown(mouseKey))
					downKeyMask |= BITMASK[mouseKey];

			// Get mouse buttons held
			foreach(KeyCode mouseKey in MOUSE_KEYS)
				if(Input.GetKey(mouseKey))
					holdKeyMask |= BITMASK[mouseKey];

			if(!_mDown.downInfo.isValid && downKeyMask != 0) // MOUSE DOWN
			{
				GetSelectables(_mRaw.downInfo.gameObject, _selectionAddList);
				Select(_selectionAddList);

				// Get modifiers held
				foreach(KeyCode modKey in MODIFIERS)
					if(Input.GetKey(modKey))
						downKeyMask |= BITMASK[modKey];

				//mDown = current;
				_mDown.keyMask = downKeyMask;
				_mDown.downTime = Time.time;
				_mDown.downInfo = _mRaw.downInfo;
				_mDown.holdInfo = default;
				_mDown.upInfo = default;
				//_mDown.Invoke(PressType.Down);
				Invoke(_mDown, PressType.Down);
			}
			else if(_mDown.downInfo.isValid && holdKeyMask != 0) // Mouse HOLD
			{
				_mDown.holdInfo = _mRaw.downInfo;
				//_mDown.Invoke(PressType.Hold);
				Invoke(_mDown, PressType.Hold);
			}
			else if(_mDown.downInfo.isValid && holdKeyMask == 0) // Mouse Up
			{
				_mDown.upInfo = _mRaw.downInfo;

				if(_mDown.downInfo.gameObject && _mDown.upInfo.gameObject && _mDown.downInfo.gameObject.Equals(_mDown.upInfo.gameObject))
					//_mDown?.Invoke(PressType.Click);
					Invoke(_mDown, PressType.Click);
				else
					//_mDown?.Invoke(PressType.Up);
					Invoke(_mDown, PressType.Up);

				_mDown.keyMask = 0u;
				_mDown.downTime = 0;
				_mDown.downInfo = default;
				_mDown.holdInfo = default;
				_mDown.upInfo = default;

			}
			else if(_mRaw.downInfo.isValid) // Mouse Hover
			{
				//_mRaw.Invoke(PressType.Hover);
				Invoke(_mRaw, PressType.Hover);
			}
		}

		static void Invoke(MouseInfo mouseInfo, PressType type)
		{
			if(!mouseInfo.downInfo.gameObject)
				return;

			mouseInfo.downInfo.gameObject.GetComponentsInParent(false, _clickableBuffer);

			foreach(IClickable click in _clickableBuffer)
				switch(type)
				{
					case PressType.Hover: click.OnMouseHover(mouseInfo); break;
					case PressType.Down: click.OnMousePress(mouseInfo); break;
					case PressType.Hold: click.OnMouseHold(mouseInfo); break;
					case PressType.Up: click.OnMouseRelease(mouseInfo); break;
					case PressType.Click: click.OnMouseClick(mouseInfo); break;
				}
		}

		static void GetSelectables(GameObject g, List<IGUISelectable> list)
		{
			Transform t = g ? g.transform : null;

			list.Clear();

			while(t)
			{
				if(t.TryGetComponent(typeof(IGUISelectable), out Component c))
					list.Add(c as IGUISelectable);

				t = t.parent;
			}

			list.Reverse();
		}

		public void Select(List<IGUISelectable> addSelection)
		{
			_selectionAddHash.Clear();

			foreach(IGUISelectable s in addSelection)
				_selectionAddHash.Add(s);

			_selectionCurrentList.Reverse();

			foreach(IGUISelectable s in _selectionCurrentList)
				if(!_selectionAddHash.Contains(s))
					s.OnDeselect();

			foreach(IGUISelectable s in addSelection)
				if(!_selectionCurrentHash.Contains(s))
					s.OnSelect();

			_selectionCurrentList.Clear();
			_selectionCurrentHash.Clear();

			foreach(IGUISelectable s in addSelection)
			{
				_selectionCurrentList.Add(s);
				_selectionCurrentHash.Add(s);
			}
		}

		public void Deselect(IGUISelectable select)
		{
			if(_selectionCurrentHash.Contains(select))
			{
				select.OnDeselect();
				_selectionCurrentHash.Remove(select);
				_selectionCurrentList.Remove(select);
			}
		}

		static void GetMouseInfo(MouseInfo mouse)
		{
			int topDepth = int.MinValue;
			RaycastResult topObject = default;

			_pointer.position = Input.mousePosition;

			EventSystem.current.RaycastAll(_pointer, _mouseCastList);
			topObject.screenPosition = Input.mousePosition;
			topObject.worldPosition = Input.mousePosition;

			for(int i = 0, ie = _mouseCastList.Count; i < ie; i++)
				if(_mouseCastList[i].depth > topDepth)
				{
					topDepth = _mouseCastList[i].depth;
					topObject = _mouseCastList[i];
				}

			_mouseCastList.Clear();

			mouse.keyMask = 0;
			mouse.downInfo = topObject;
			mouse.holdInfo = default;
			mouse.upInfo = default;
			mouse.downTime = Time.time;
		}
	}

	public enum PressType { Hover, Down, Hold, Up, Click }
	public class MouseInfo
	{ 
		public uint keyMask;
		public RaycastResult downInfo;
		public RaycastResult holdInfo;
		public RaycastResult upInfo;
		public float downTime;

		public bool Contains(KeyCode press)
		{
			if(GUIInputManager.BITMASK.TryGetValue(press, out uint bit))
				return (keyMask & bit) != 0;
			else
				return false;
		}
	}
}