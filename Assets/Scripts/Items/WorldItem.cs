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
        
        /// NOT SYNCED ONLY AVAILABLE ON SERVER
        private Transform _anchor;

        private void LateUpdate()
        {
            if (!IsServer) return;
            if (_anchor == null) return;
            
            transform.position = _anchor.position;
            transform.rotation = Quaternion.identity;
        }

        public bool PickUp(NetworkInventory inventory, NetworkObject parent, string transformPath)
        {
            if (!_pickable) return false;
            if (!inventory.AddItem(this)) return false;

            PickUpRpc(inventory);
            SetAnchorRpc(parent, transformPath);
            
            return true;
        }

        [Rpc(SendTo.Server)]
        private void SetAnchorRpc(NetworkObjectReference parent, string transformPath)
        {
            if (!parent.TryGet(out var obj)) return;
            _anchor = string.IsNullOrEmpty(transformPath) ? obj.transform : obj.transform.Find(transformPath);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PickUpRpc(NetworkBehaviourReference inventory)
        {
            if (!inventory.TryGet(out NetworkInventory inv, NetworkManager)) return;
            
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            col.enabled = false;
            _inventory = inv;
            _pickable = false;
        }

        public bool Drop()
        {
            if (_pickable) return false;
            
            _anchor = null;
            _inventory.RemoveItem(this);
            
            DropRpc();
            ClearAnchorRpc();
            
            return true;
        }

        [Rpc(SendTo.Server)]
        private void ClearAnchorRpc()
        {
            _anchor = null;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void DropRpc()
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            col.enabled = true;
            _inventory = null;
            _pickable = true;
        }
    }
}