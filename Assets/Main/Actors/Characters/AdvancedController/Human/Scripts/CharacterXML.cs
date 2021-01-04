using MPCore;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    [System.Serializable]
    [XMLSurrogate(typeof(Character))]
    public class CharacterXML : XMLSurrogate
    {
        public bool isPlayer;
        public List<InventoryItemXML> inventory;
        public ResourceItemXML[] resources;

        [System.Serializable]
        public struct ResourceItemXML
        {
            public string resourcePath;
            public int value;
            public int maxValue;
        }

        [System.Serializable]
        public struct InventoryItemXML
        {
            public string resourcePath;
            public bool staticReference;
            public int count;
            public int maxCount;
        }

        public override XMLSurrogate Serialize(dynamic o)
        {
            if (o is Character character)
            {
                isPlayer = character.isPlayer;

                // Inventory
                inventory = new List<InventoryItemXML>(character.inventory.Count);
                foreach (Inventory item in character.inventory)
                    inventory.Add(new InventoryItemXML()
                    {
                        resourcePath = item.resourcePath,
                        staticReference = item.staticReference,
                        count = item.count,
                        maxCount = item.maxCount
                    });

                // Resources
                resources = new ResourceItemXML[character.resources.Count];
                for (int i = 0; i < resources.Length; i++)
                    resources[i] = new ResourceItemXML()
                    {
                        resourcePath = character.resources[i].resourceType.resourcePath,
                        value = character.resources[i].value,
                        maxValue = character.resources[i].maxValue
                    };
            }

            return this;
        }

        public override XMLSurrogate Deserialize(dynamic o)
        {
            if(o is Character character && character)
            {
                // Inventory
                character.inventory.Clear();
                foreach(InventoryItemXML item in inventory)
                    if (Resources.Load<ScriptableObject>(item.resourcePath) is Inventory inv && inv)
                    {
                        if (!item.staticReference)
                        {
                            inv = ScriptableObject.Instantiate(inv);
                            inv.maxCount = item.maxCount;
                            inv.count = item.count;
                        }

                        InventoryManager.PickUp(character, inv);
                    }
                    else
                        Debug.LogWarning($"Resource error: missing {item.resourcePath}");

                // Resources
                character.resources.Clear();
                foreach (ResourceItemXML res in resources)
                    if (Resources.Load<ScriptableObject>(res.resourcePath) is ResourceType rtype && rtype)
                        character.resources.Add(new ResourceValue()
                        {
                            resourceType = rtype,
                            value = res.value,
                            maxValue = res.maxValue
                        });
                    else
                        Debug.LogWarning($"Resource error: missing {res.resourcePath}");

                character.SetAsCurrentPlayer(isPlayer);
            }

            return this;
        }
    }
}
