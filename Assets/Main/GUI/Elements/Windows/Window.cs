using MPCore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
	/// <summary>
	/// A generic, multi-purpose GUI window
	/// </summary>
	public class Window : MonoBehaviour, IClickable
	{
		static readonly List<Window> _tryGetWindows = new();

		[SerializeField] RectTransform panel;
		[SerializeField] RectTransform title;
		[SerializeField] TMP_Text _titleText;
		[SerializeField] Image _titleBackground;
		[SerializeField] bool pauseWhenActive = true;
		public DataValue<bool> active = new(true);

		GameModel _gameModel;
		GUIModel _guiModel;

		public string Title
		{
			get => _titleText.text;
			set => _titleText.SetText(value);
		}

		void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_guiModel = Models.GetModel<GUIModel>();

			active.Subscribe(SetWindowActive);
		}

		void OnEnable()
		{
			active.Value = true;

			if (pauseWhenActive)
				_gameModel.pauseTickets.Value++;
		}

		void OnDisable()
		{
			if (active.Value)
				active.Value = false;

			if (pauseWhenActive)
				_gameModel.pauseTickets.Value--;
		}

		public void TogleActive()
		{
			gameObject.SetActive(!gameObject.activeSelf);
		}

		public void CloseWindow()
		{
			int sibling = transform.parent.childCount;
			int thisIndex = transform.GetSiblingIndex();
			Window newTop = null;

			while(sibling-- > 0)
				if(sibling != thisIndex)
				{
					Transform child = transform.parent.GetChild(sibling);

					if (child.TryGetComponent(out newTop))
						break;
				}

			if (newTop)
				newTop.active.Value = true;

			Destroy(gameObject);
		}

		void SetWindowActive(DeltaValue<bool> isActive)
		{
			WindowStyle style = _guiModel.style.Value;
			bool value = isActive.newValue;

			if (isActive.newValue != isActive.oldValue)
			{
				_titleBackground.sprite = value ? style.activeTitleBackground : style.inactiveTitleBackground;
				_titleText.color = value ? style.activeTextColor : style.inactiveTextColor;

				if (value == true)
				{
					// Bring to front
					transform.SetAsLastSibling();

					// Deactivate Sibling Windows
					if (transform.parent)
					{
						transform.parent.GetComponentsInChildren(true, _tryGetWindows);

						foreach(Window window in _tryGetWindows)
							if(window != this)
								window.active.Value = false;
					}
				}
			}
		}

		public void OnMouseClick(MouseInfo mouse) { }
		public void OnMouseHold(MouseInfo mouse) { }
		public void OnMouseHover(MouseInfo mouse) { }
		public void OnMouseRelease(MouseInfo mouse) { }
		public void OnMousePress(MouseInfo mouse)
		{
			active.Value = true;
		}

		/// <summary>
		/// Spawns a clone of this <c>Window</c> under the given parent
		/// </summary>
		/// <param name="parent"><c>RectTransform</c> under which to spawn the <c>Window</c></param>
		public void SpawnWindow(RectTransform parent)
		{
			if (!TryGetDuplicate(parent, out Window dupe))
			{
				Window instance = Instantiate(this, parent);
				RectTransform rt = instance.transform as RectTransform;

				rt.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
			}
			else
			{
				dupe.active.Value = true;
			}
		}

		/// <summary>
		/// Checks children of <c>parent</c> to see if any windows have the same title as this one
		/// </summary>
		/// <param name="parent"><c>RectTransform</c> under which to search for duplicates</param>
		/// <returns><c>true</c> if this window is already spawned under <c>parent</c></returns>
		bool TryGetDuplicate(RectTransform parent, out Window dupe)
		{
			parent.GetComponentsInChildren(true, _tryGetWindows);

			foreach(Window window in _tryGetWindows)
				if(window.Title == Title)
				{
					dupe = window;
					return true;
				}

			dupe = null;
			return false;
		}
	}
}
