using Unity.Netcode;
using UnityEngine;

namespace Items
{
    public class NetworkInventory : NetworkBehaviour
    {
        [SerializeField] private int capacity;

        private NetworkList<NetworkObjectReference> _items = new(writePerm: NetworkVariableWritePermission.Owner);

        public bool AddItem(WorldItem itemType)
        {
            if (capacity != -1 && _items.Count >= capacity)
            {
                return false;
            }

            _items.Add(itemType.NetworkObject);
            return true;
        }

        public void RemoveItem(WorldItem item)
        {
            _items.Remove(item.NetworkObject);
        }
    }
}