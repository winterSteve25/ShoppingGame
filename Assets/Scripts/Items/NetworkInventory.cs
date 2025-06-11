using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Items
{
    public class NetworkInventory : NetworkBehaviour
    {
        [SerializeField] private List<WorldItem> items;
        [SerializeField] private int capacity;
        
        public bool AddItem(WorldItem itemType)
        {
            if (items.Count >= capacity)
            {
                return false;
            }

            AddItemRpc(itemType);
            return true;
        }
        
        public void RemoveItem(WorldItem item)
        {
            RemoveItemRpc(item);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RemoveItemRpc(NetworkBehaviourReference item)
        {
            if (!item.TryGet(out WorldItem wi)) return;
            items.Remove(wi);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void AddItemRpc(NetworkBehaviourReference item)
        {
            if (!item.TryGet(out WorldItem wi)) return;
            items.Add(wi);
        }
    }
}