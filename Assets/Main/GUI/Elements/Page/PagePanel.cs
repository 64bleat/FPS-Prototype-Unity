using MPCore;
using System;
using UnityEngine;

namespace MPGUI
{
	/// <summary> Activates a given GameObject and deactivates all other siblings. </summary>
	public class PagePanel : MonoBehaviour
	{
		[SerializeField] SwitchBind[] _keyBinds = new SwitchBind[0];
		[SerializeField] PagePanel _firstPage;
		[SerializeField] bool _pauseWhenEnabled = false;

		GameModel _gameModel;
		InputManager _input;

		[Serializable]
		public struct SwitchBind
		{
			public KeyBind switchKey;
			public PagePanel switchTo;
		}

		void OnValidate()
		{
			// This must be the parent
			if (_firstPage && _firstPage.transform.parent != transform)
				_firstPage = null;
		}

		void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_input = GetComponentInParent<InputManager>();
		}

		void OnEnable()
		{
			foreach (SwitchBind bind in _keyBinds)
				_input.Bind(bind.switchKey.name, bind.switchTo.SwitchToThis, this, KeyPressType.Down);

			if (_firstPage)
				_firstPage.SwitchToThis();

			if (_pauseWhenEnabled)
				_gameModel.pauseTickets.Value++;
		}

		void OnDisable()
		{
			_input.Unbind(this);

			if (_pauseWhenEnabled)
				_gameModel.pauseTickets.Value--;
		}

		/// <summary> Make all other transforms in the target's child group inactive. </summary>
		void Switch(PagePanel target)
		{
			Transform parent = target.transform.parent;

			if(parent)
				for (int i = 0, count = parent.childCount; i < count; i++)
					if (parent.GetChild(i).TryGetComponent(out PagePanel sibPage))
						sibPage.gameObject.SetActive(sibPage == this);

			if (target._firstPage)
				target._firstPage.SwitchToThis();
		}

		public void SwitchToThis()
		{
			Switch(this);
		}
	}
}
