using Unity.Netcode;
using UnityEngine;

namespace Items
{
    public class WorldItem : NetworkBehaviour
    {
        public ItemType ItemType => itemType;
        
        [SerializeField] private ItemType itemType;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider col;

        /// SYNCED
        private NetworkInventory _inventory;
        private bool _pickable = true;
        
        /// NOT SYNCED ONLY AVAILABLE ON OWNER
        private Transform _anchor;

        private void LateUpdate()
        {
            if (!IsOwner) return;
            if (_anchor == null) return;
            
            transform.position = _anchor.position;
            transform.rotation = Quaternion.identity;
        }
        
        public bool PickUp(NetworkInventory inventory, Transform anchor)
        {
            if (!IsOwner)
            {
                Debug.Log("CAN ONLY BE PICKED UP BY NETWORK OWNER");
                return false;
            }
            
            if (!_pickable) return false;
            if (!inventory.AddItem(this)) return false;
            
            _anchor = anchor;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;

            PickUpRpc(inventory);
            
            return true;
        }
        
        public bool Drop()
        {
            if (!IsOwner)
            {
                Debug.Log("CAN ONLY BE PICKED UP BY NETWORK OWNER");
                return false;
            }
            
            if (_pickable) return false;
            
            _anchor = null;
            _inventory.RemoveItem(this);
            
            rb.isKinematic = false;
            rb.useGravity = true;
            
            DropRpc();
            return true;
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        private void PickUpRpc(NetworkBehaviourReference inventory)
        {
            if (!inventory.TryGet(out NetworkInventory inv, NetworkManager)) return;
            
            col.enabled = false;
            _inventory = inv;
            _pickable = false;
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        private void DropRpc()
        {
            col.enabled = true;
            _inventory = null;
            _pickable = true;
        }
    }
}