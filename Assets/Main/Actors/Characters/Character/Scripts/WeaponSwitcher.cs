using System;
using UnityEngine;

namespace MPCore
{
	/// <summary> Handles drawing and holstering of weapons. </summary>
	[Serializable]
	public class WeaponSwitcher : MonoBehaviour
	{
		public bool drawOnStart = true;
		public Weapon currentWeapon;
		public Weapon lastWeapon;
		public RectTransform emptyCrosshair;

		InventoryManager _inventory;
		Character _character;
		CharacterBody _body;
		InputManager _input;
		GUIModel _guiModel;
		GameObject _currentWeaponGameObject;
		Inventory _currentPassive;
		int _currentWeaponSlot;

		void Awake()
		{
			_guiModel = Models.GetModel<GUIModel>();
			_inventory = GetComponent<InventoryManager>();
			_character = GetComponentInParent<Character>();
			_body = _character.GetComponent<CharacterBody>();
			_input = GetComponent<InputManager>();

			_character.OnInitialized.AddListener(Initialize);
			_input.Bind("LastWeapon", DrawLastWeapon, this);
			_input.Bind("WeaponSlot1", () => DrawWeapon(1), this);
			_input.Bind("WeaponSlot2", () => DrawWeapon(2), this);
			_input.Bind("WeaponSlot3", () => DrawWeapon(3), this);
			_input.Bind("WeaponSlot4", () => DrawWeapon(4), this);
			_input.Bind("WeaponSlot5", () => DrawWeapon(5), this);
			_input.Bind("WeaponSlot6", () => DrawWeapon(6), this);
			_input.Bind("WeaponSlot7", () => DrawWeapon(7), this);
			_input.Bind("WeaponSlot8", () => DrawWeapon(8), this);
			_input.Bind("WeaponSlot9", () => DrawWeapon(9), this);
			_input.Bind("WeaponSlot0", () => DrawWeapon(0), this);
			_input.Bind("ItemActivate", ToggleSelectedActivatable, this);
			_input.Bind("ItemNext", NextActivatable, this);
			_input.Bind("ItemPrevious", PreviousActivatable, this);
			_input.OnMouseScrollVertical.AddListener(DrawScroll);
		}

		void Start()
		{
			if (currentWeapon)
				DrawWeapon(currentWeapon);
			else if (drawOnStart)
				DrawBestWeapon();
		}

		void OnDestroy()
		{
			_input.OnMouseScrollVertical.RemoveListener(DrawScroll);

			if (_character.IsPlayer)
				_guiModel.crosshair.Value = null;
		}

		void Initialize(bool isPlayer)
		{
			if (isPlayer && currentWeapon)
				_guiModel.crosshair.Value = currentWeapon.crosshair;
		}

		public void NextActivatable()
		{
			SelectActivatable(true);
		}

		void PreviousActivatable()
		{
			SelectActivatable(false);
		}

		void SelectActivatable(bool forward)
		{
			int count = _inventory.inventory.Count;
			int start;
			Inventory next = null;

			if (_currentPassive)
				start = _inventory.inventory.IndexOf(_currentPassive) + (forward ? 1 : -1);
			else
				start = 0;

			for (int i = 0; i < count; i++)
			{
				Inventory item;

				if (forward)
					item = _inventory.inventory[(start + i) % count];
				else
					item = _inventory.inventory[(start + count - i) % count];

				if (item.activatable)
				{
					next = item;
					break;
				}
			}

			if (next)
			{
				_currentPassive = next;

				if (_character.IsPlayer)
					_guiModel.currentPassive.Value = next;
			}
		}

		public void ToggleSelectedActivatable()
		{
			if (_currentPassive)
			{
				_currentPassive.SetActive(_character.gameObject, !_currentPassive.active);

				if (_character.IsPlayer)
					_guiModel.PassiveToggle?.Invoke(_currentPassive);
			}
		}

		void DrawScroll(float scroll)
		{
			int count = _inventory.inventory.Count;
			Weapon next = null;
			Weapon loop = null;

			for (int i = 0; i < count; i++)
				if (_inventory.inventory[i] is Weapon w)
					if (scroll > 0)
					{
						if (w.weaponSlot > currentWeapon.weaponSlot && (!next || w.weaponSlot < next.weaponSlot))
							next = w;
						else if (w.weaponSlot < currentWeapon.weaponSlot && (!loop || w.weaponSlot < loop.weaponSlot))
							loop = w;
					}
					else if (scroll < 0)
					{
						if (w.weaponSlot < currentWeapon.weaponSlot && (!next || w.weaponSlot > next.weaponSlot))
							next = w;
						else if (w.weaponSlot > currentWeapon.weaponSlot && (!loop || w.weaponSlot > loop.weaponSlot))
							loop = w;
					}

			if (!next)
				next = loop;

			if (next)
				DrawWeapon(next);
		}

		/// <summary> Draw an automatically picked weapon </summary>
		public void DrawBestWeapon()
		{
			(Weapon weapon, float priority) next = (null, float.NegativeInfinity);

			foreach (Inventory item in _inventory.inventory)
				if (item is Weapon weapon)
					if (weapon.weaponSlot > next.priority)
						next = (weapon, weapon.weaponSlot);

			DrawWeapon(next.weapon);
		}

		/// <summary> Draw an automatically picked weapon that isn't currentWeapon </summary>
		public void DrawNextBestWeapon()
		{
			(Weapon weapon, float priority) next = (null, -1);

			foreach (Inventory item in _inventory.inventory)
				if (item is Weapon weapon && weapon != currentWeapon)
					if (weapon.weaponSlot > next.priority)
						next = (weapon, weapon.weaponSlot);

			DrawWeapon(next.weapon);
		}

		/// <summary> Draw the previously drawn weapon if it exists </summary>
		public void DrawLastWeapon()
		{
			if(lastWeapon && _inventory.inventory.Contains(lastWeapon))
				DrawWeapon(lastWeapon);
		}

		/// <summary> Draws a weapon in the provided slot if one exists </summary>
		public void DrawWeapon(int slot)
		{ 
			if (_currentWeaponSlot != slot)
			{
				Weapon nextWeapon = null;

				foreach (Inventory item in _inventory.inventory)
					if (item is Weapon weapon && weapon.weaponSlot == slot)
						nextWeapon = weapon;

				if(nextWeapon)
					DrawWeapon(nextWeapon);
			}
		}

		/// <summary> Draw any provided weapon </summary>
		/// <remarks> If no weapon exists, currentWeapon will still be holstered. </remarks>
		public void DrawWeapon(Weapon weapon)
		{
			if (currentWeapon)
				if (weapon == currentWeapon)
					return;
				else
				{
					lastWeapon = currentWeapon;
					Destroy(_currentWeaponGameObject);
				}

			if (weapon)
			{
				Transform weaponHand;
				switch (weapon.weaponHolder)
				{
					case Weapon.WeaponHolder.RightHand:
						weaponHand = _body.rightHand;
						break;
					case Weapon.WeaponHolder.LeftHand:
						weaponHand = _body.leftHand;
						break;
					case Weapon.WeaponHolder.Center:
						weaponHand = _body.cameraHand;
						break;
					case Weapon.WeaponHolder.Camera:
						weaponHand = _body.cameraHand;
						break;
					default:
						weaponHand = _body.rightHand;
						break;
				}

				_currentWeaponGameObject = Instantiate(weapon.firstPersonPrefab, weaponHand, false);

				if(_currentWeaponGameObject.TryGetComponent(out InventoryItem ii))
					ii.item = weapon;

				if (_currentWeaponGameObject.TryGetComponent(out RocketLauncherEquip rle))
					rle.weapon = weapon;

				_currentWeaponSlot = weapon.weaponSlot;

				if (_character.IsPlayer)
				{
					_guiModel.crosshair.Value = weapon.crosshair;
					_guiModel.currentWeapon.Value = weapon;
				}
			}
			else
			{
				_currentWeaponGameObject = null;
				_currentWeaponSlot = -1;

				if (_character.IsPlayer)
				{
					_guiModel.crosshair.Value = emptyCrosshair;
					_guiModel.currentWeapon.Value = null;
				}
			}

			currentWeapon = weapon;
		}
	}
}