using Items;
using Unity.Netcode;
using UnityEngine;

namespace Utils
{
    public class ItemStack2DArray : INetworkSerializable
    {
        public int Dimension => _dimension;
        
        private int _dimension;
        private Vector2Int[] _items;

        public ItemStack2DArray()
        {
        }

        public ItemStack2DArray(int dimension)
        {
            _dimension = dimension;
            _items = new Vector2Int[dimension * dimension];
        }

        public ItemStack this[int x, int y]
        {
            get => Unpack(_items[_dimension * y + x]);
            set => _items[_dimension * y + x] = Pack(value);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _dimension);
            serializer.SerializeValue(ref _items);
        }

        private ItemStack Unpack(Vector2Int item)
        {
            var id = item.x;
            var amount = item.y;
            var it = ItemTypeManager.ItemTypes[id];
            return new ItemStack(it, amount);
        }

        private Vector2Int Pack(ItemStack item)
        {
            var id = ItemTypeManager.ItemTypes.IndexOf(item.Item);
            var amount = item.Amount;
            return new Vector2Int(id, amount);
        }
    }
}