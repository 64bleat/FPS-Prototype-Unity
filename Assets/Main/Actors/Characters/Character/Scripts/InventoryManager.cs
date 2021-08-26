//using System.Collections.Generic;
//using UnityEngine;

//namespace MPCore
//{
//    [System.Obsolete("Use InventoryContainer Directly!!!")]
//    public static class InventoryManager
//    {
//        public static void Remove(List<Inventory> list, Inventory inventoryType, int count)
//        {
//            if(list != null && inventoryType != null && count > 0
//                && list.Find((i) => i.displayName.Equals(inventoryType.displayName)) is var item && item)
//            {
//                item.count -= count;

//                if (item.count <= 0)
//                    list.Remove(item);
//            }
//        }
//    }
//}
