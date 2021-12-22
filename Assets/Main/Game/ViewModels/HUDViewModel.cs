using MPGUI;
using Serialization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
	/// <summary>
	/// Displays the GUIModel as a HUD
	/// </summary>
	public class HUDViewModel : MonoBehaviour
	{
		const float KILL_MESSAGE_TIMER = 3f;

		[SerializeField] RectTransform _hudParent;
		[SerializeField] RectTransform _crosshairParent;
		[SerializeField] TMP_Text _shortMessageArea;
		[SerializeField] TMP_Text _largeMessageArea;
		[SerializeField] TMP_Text _timer;
		[SerializeField] TMP_Text _speed;
		[SerializeField] TMP_Text _health;
		[SerializeField] RectTransform _killMessageArea;
		[SerializeField] GameObject _killMessageTemplate;
		[SerializeField] ItemSlotViewModel[] _weaponSlots;
		[SerializeField] PassiveItemViewModel _passiveTemplate;
		[SerializeField] Window _inventoryWindow;
		[SerializeField] Window _consoleWindow;

		GameModel _gameModel;
		TimeModel _timeModel;
		GUIModel _guiModel;
		SaveModel _saveModel;
		InputManager _input;

		readonly Dictionary<Inventory, PassiveItemViewModel> _passives = new();
		RectTransform _crosshair;

		void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_guiModel = Models.GetModel<GUIModel>();
			_saveModel = Models.GetModel<SaveModel>();
			_timeModel = Models.GetModel<TimeModel>();
			_input = GetComponentInParent<InputManager>();

			_guiModel.timer.Value = 0;
			_guiModel.shortMessage.Subscribe(DisplayShortMessage, false);
			_guiModel.largeMessage.Subscribe(DisplayLargeMessage, false);
			_guiModel.timer.Subscribe(DisplayTimer);
			_guiModel.speed.Subscribe(DisplaySpeed);
			_guiModel.health.Subscribe(DisplayHealth);
			_guiModel.killMessage.Subscribe(DisplayKillMessage, false);
			_guiModel.OpenWindow.AddListener(OpenWindow);
			_guiModel.crosshair.Subscribe(SetCrosshair);
			_guiModel.currentWeapon.Subscribe(SelectWeapon);
			_guiModel.WeaponPickup.AddListener(PickupWeapon);
			_guiModel.WeaponDrop.AddListener(DropWeapon);
			_guiModel.currentPassive.Subscribe(PassiveSelected);
			_guiModel.PassivePickup.AddListener(PassivePickup);
			_guiModel.PassiveDrop.AddListener(PassiveDrop);
			_guiModel.PassiveToggle.AddListener(PassiveToggle);
			//_saveModel.OnPostLoad.AddListener(MessageQuickLoad);
			//_saveModel.OnPostSave.AddListener(MessageQuickSave);
			_timeModel.currentTime.Subscribe(SetTime);

			MessageBus.Subscribe<SaveModel.PostSave>(MessageQuickSave);
			MessageBus.Subscribe<SaveModel.PostLoad>(MessageQuickLoad);
		}

		void OnEnable()
		{
			_input.Bind("Console", ToggleActiveConsole, this);
			_input.Bind("Inventory", ToggleActiveInventory, this);
		}

		private void OnDisable()
		{
			_input.Unbind(this);
		}

		void OnDestroy()
		{
			_guiModel.shortMessage.Unsubscribe(DisplayShortMessage);
			_guiModel.largeMessage.Unsubscribe(DisplayLargeMessage);
			_guiModel.timer.Unsubscribe(DisplayTimer);
			_guiModel.speed.Unsubscribe(DisplaySpeed);
			_guiModel.health.Unsubscribe(DisplayHealth);
			_guiModel.killMessage.Unsubscribe(DisplayKillMessage);
			_guiModel.OpenWindow.RemoveListener(OpenWindow);
			_guiModel.crosshair.Unsubscribe(SetCrosshair);
			_guiModel.currentWeapon.Unsubscribe(SelectWeapon);
			_guiModel.WeaponPickup.RemoveListener(PickupWeapon);
			_guiModel.WeaponDrop.RemoveListener(DropWeapon);
			_guiModel.currentPassive.Unsubscribe(PassiveSelected);
			_guiModel.PassivePickup.RemoveListener(PassivePickup);
			_guiModel.PassiveDrop.RemoveListener(PassiveDrop);
			_guiModel.PassiveToggle.RemoveListener(PassiveToggle);
			_timeModel.currentTime.Unsubscribe(SetTime);

			MessageBus.Unsubscribe<SaveModel.PostSave>(MessageQuickSave);
			MessageBus.Unsubscribe<SaveModel.PostLoad>(MessageQuickLoad);
		}

		void ToggleActiveInventory()
		{
			if (!_gameModel.isPaused.Value && !_inventoryWindow.gameObject.activeSelf)
				_inventoryWindow.gameObject.SetActive(true);
			else if (_inventoryWindow.active)
				_inventoryWindow.gameObject.SetActive(false);
		}
		void ToggleActiveConsole()
		{
			_consoleWindow.gameObject.SetActive(!_consoleWindow.gameObject.activeSelf);
		}

		void MessageQuickSave(SaveModel.PostSave _) => DisplayLargeMessage(new DeltaValue<string>(string.Empty, "Quicksaved"));
		void MessageQuickLoad(SaveModel.PostLoad _) => DisplayLargeMessage(new DeltaValue<string>(string.Empty, "Quickloaded"));
		void DisplayShortMessage(DeltaValue<string> message) => DisplayMessageShared(message, _shortMessageArea);
		void DisplayLargeMessage(DeltaValue<string> message) => DisplayMessageShared(message, _largeMessageArea);
		void DisplayTimer(DeltaValue<float> timer) => _timer.SetText(timer.newValue == 0 ? string.Empty : timer.newValue.ToString("N2"));
		void DisplaySpeed(DeltaValue<float> speed) => _speed.SetText(speed.newValue.ToString("F0"));
		void DisplayHealth(DeltaValue<int> health) => _health.SetText(health.newValue.ToString());
		void SetTime(DeltaValue<DateTime> date)
		{
			if (date.oldValue.Minute != date.newValue.Minute || date.oldValue.Hour != date.newValue.Hour)
				_timer.SetText(date.newValue.ToShortTimeString());
		}

		void DisplayMessageShared(DeltaValue<string> message, TMP_Text text)
		{
			text.gameObject.SetActive(false);
			text.SetText(message.newValue);
			text.gameObject.SetActive(true);
		}

		void OpenWindow(Window reference)
		{
			Window window = Instantiate(reference, _hudParent);

			window.gameObject.SetActive(true);
		}

		void SetCrosshair(DeltaValue<RectTransform> crosshair)
		{
			if (_crosshair)
				Destroy(_crosshair.gameObject);

			if(crosshair.newValue)
				_crosshair = Instantiate(crosshair.newValue, _crosshairParent);
		}

		void DisplayKillMessage(DeltaValue<MessageEventParameters> messageInfo)
		{
			var message = messageInfo.newValue;

			GameObject go = Instantiate(_killMessageTemplate, _killMessageArea);

			if (go.TryGetComponentInChildren(out Image image))
				image.color = message.bgColor;

			if (go.TryGetComponentInChildren(out TextMeshProUGUI text))
			{
				text.SetText(message.message);
				text.color = message.color;
			}

			go.SetActive(true);
			Destroy(go, KILL_MESSAGE_TIMER);
		}

		void SelectWeapon(DeltaValue<Weapon> weapon)
		{
			if(weapon.oldValue)
				_weaponSlots[weapon.oldValue.weaponSlot].SetInactive();

			if(weapon.newValue)
				_weaponSlots[weapon.newValue.weaponSlot].SetActive();
		}

		void PickupWeapon(Weapon weapon)
		{
			if(weapon)
				_weaponSlots[weapon.weaponSlot].SetWeapon(weapon);
		}

		void DropWeapon(Weapon weapon)
		{
			if (weapon)
				_weaponSlots[weapon.weaponSlot].SetWeapon(null);
		}

		private void PassivePickup(Inventory item)
		{
			PassiveItemViewModel passive = Instantiate(_passiveTemplate, _passiveTemplate.transform.parent);

			passive.SetItem(item);
			_passives.Add(item, passive);
			passive.gameObject.SetActive(true);
		}

		private void PassiveDrop(Inventory item)
		{
			if (_passives.TryGetValue(item, out PassiveItemViewModel passive))
			{
				Destroy(passive.gameObject);
				_passives.Remove(item);
			}
		}

		private void PassiveToggle(Inventory item)
		{
			if (_passives.TryGetValue(item, out PassiveItemViewModel passive))
				if (item.active)
					passive.SetActive();
				else
					passive.SetInactive();
		}

		private void PassiveSelected(DeltaValue<Inventory> item)
		{
			PassiveItemViewModel passive;

			if (item.oldValue && _passives.TryGetValue(item.oldValue, out passive))
				passive.Deselect();

			if (item.newValue && _passives.TryGetValue(item.newValue, out passive))
				passive.Select();
		}
	}
}
