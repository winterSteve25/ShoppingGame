using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Items
{
    public class WorldItem : NetworkBehaviour
    {
        public ItemType ItemType
        {
            get => ItemTypeManager.ItemTypes[_itemType.Value];
            set => _itemType.Value = ItemTypeManager.ItemTypes.IndexOf(value);
        }

        [ShowInInspector]
        public bool IsOnTree
        {
            get => _isOnTree.Value;
            set => _isOnTree.Value = value;
        }
        
        public event Action<WorldItem> OnPicked;

        [SerializeField] private ItemType itemType;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private Collider col;
        [SerializeField] protected Rigidbody rb;

        /// SYNCED
        private NetworkVariable<int> _itemType = new(-1, writePerm: NetworkVariableWritePermission.Server);
        private NetworkVariable<bool> _isOnTree = new(false, writePerm: NetworkVariableWritePermission.Server);
        private NetworkInventory _inventory;
        private bool _pickable = true;

        /// NOT SYNCED ONLY AVAILABLE ON OWNER
        private Transform _anchor;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _itemType.OnValueChanged += (_, newValue) => UpdateTransform(newValue);
            
            if (IsServer && itemType != null)
            {
                _itemType.Value = ItemTypeManager.ItemTypes.IndexOf(itemType);
            }

            if (_itemType.Value != -1)
            {
                UpdateTransform(_itemType.Value);
            }
        }

        private void UpdateTransform(int newValue)
        {
            var value = ItemTypeManager.ItemTypes[newValue];
            transform.localScale = 0.8f * value.Scale;

            if (value.Mesh != null)
            {
                meshFilter.mesh = value.Mesh;
                meshCollider.sharedMesh = value.Mesh;
            }

            if (value.Materials.Length > 0)
            {
                meshRenderer.materials = value.Materials;
            }
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;
            if (_anchor == null) return;

            transform.position = _anchor.position;
            transform.rotation = Quaternion.LookRotation(_anchor.forward);
        }

        public bool PickUp(NetworkInventory inventory, Transform anchor)
        {
            if (!IsOwner)
            {
                Debug.LogWarning("CAN ONLY BE PICKED UP BY NETWORK OWNER");
                return false;
            }

            if (!_pickable) return false;
            if (!inventory.AddItem(this)) return false;

            SetAnchor(anchor);
            PickUpRpc(inventory);

            return true;
        }

        public void SetAnchor(Transform anchor)
        {
            if (!IsOwner)
            {
                Debug.LogWarning("ANCHOR CAN ONLY BE SET BY NETWORK OWNER");
                return;
            }

            _anchor = anchor;

            if (!rb.isKinematic)
            {
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
            }

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        public virtual bool Drop()
        {
            if (!IsOwner)
            {
                Debug.LogWarning("CAN ONLY BE DROPPED BY NETWORK OWNER");
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
            OnPicked?.Invoke(this);
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