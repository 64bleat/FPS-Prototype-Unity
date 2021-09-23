using MPWorld;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
	public class InventoryExchangeEvent : MonoBehaviour , IInteractable
	{
		public List<ItemExchange> itemExchange;
		public UnityEvent exchangeEvents;
		public bool consumeItems = true;

		[System.Serializable]
		public class ItemExchange
		{
			public Inventory item;
			public int count = 1;
		}

		public void OnInteractEnd(GameObject other, RaycastHit hit) { }
		public void OnInteractHold(GameObject other, RaycastHit hit) { }
		public void OnInteractStart(GameObject other, RaycastHit hit)
		{
			if(other.TryGetComponentInChildren(out InventoryManager container))
			{
				HashSet<ItemExchange> fulfilled = new HashSet<ItemExchange>();

				foreach (ItemExchange exchange in itemExchange)
					if (container.TryFind(exchange.item, out Inventory item) && item.count >= exchange.count)
						fulfilled.Add(exchange);

				if (fulfilled.Count == itemExchange.Count)
				{
					if (consumeItems)
						foreach (ItemExchange exchange in fulfilled)
							container.Remove(exchange.item, exchange.count);

					exchangeEvents.Invoke();
				}
			}
		}
	}
}
