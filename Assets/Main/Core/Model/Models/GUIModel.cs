using MPGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Dropdown = MPGUI.Dropdown;

namespace MPCore
{
	public class GUIModel : Models
	{
		public Dropdown dropdown;
		public DataValue<WindowStyle> style = new();
		public DataValue<RectTransform> crosshair = new();
		public DataValue<string> shortMessage = new();
		public DataValue<string> largeMessage = new();
		public DataValue<float> timer = new();
		public DataValue<float> speed = new();
		public DataValue<int> health = new();
		public DataValue<MessageEventParameters> killMessage = new();
		public DataValue<Weapon> currentWeapon = new();
		public DataValue<Inventory> currentPassive = new();
		public InventorySlot[] weaponSlots = new InventorySlot[10];
		public List<InventorySlot> inventorySlots = new();

		public UnityEvent<Window> OpenWindow = new();
		public UnityEvent<Weapon> WeaponPickup = new();
		public UnityEvent<Weapon> WeaponDrop = new();
		public UnityEvent<Weapon> WeaponSelect = new();
		public UnityEvent<Inventory> PassivePickup = new();
		public UnityEvent<Inventory> PassiveDrop = new();
		public UnityEvent<Inventory> PassiveToggle = new();

		[Serializeable]
		public class InventorySlot
		{
			public enum Selection { Inactive, Active, Disabled, Selected}
			public DataValue<Selection> selection = new();
			public DataValue<string> name = new();
			public DataValue<Image> icon = new();
			public DataValue<Image> altIcon = new();
			public DataValue<int> count = new();
			public DataValue<int> maxCount = new();
		}

		void Awake()
		{
			for (int i = 0; i < weaponSlots.Length; i++)
				weaponSlots[i] = new();
		}
	}

	[Serializable]
	public struct MessageEventParameters
	{
		public string message;
		public Color color;
		public Color bgColor;
	}
}