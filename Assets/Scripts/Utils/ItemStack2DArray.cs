using System;
using Items;
using Unity.Collections;
using Unity.Netcode;

namespace Utils
{
    public class ItemStack2DArray : INetworkSerializable, IDisposable
    {
        private int _width;
        private NativeArray<ulong> _items;

        public ItemStack2DArray()
        {
        }

        public ItemStack2DArray(int width, int height)
        {
            _width = width;
            _items = new NativeArray<ulong>(width * height, Allocator.Persistent);
        }

        public ItemStack this[int x, int y]
        {
            get => Unpack(_items[_width * y + x]);
            set => _items[_width * y + x] = Pack(value);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _width);
            serializer.SerializeValue(ref _items, Allocator.Persistent);
        }

        public void Dispose()
        {
            _items.Dispose();
        }

        private ItemStack Unpack(ulong item)
        {
            var id = (int)(item >> 32);
            var amount = (int)(item & 0xFFFFFFFF);
            var it = ItemTypeManager.ItemTypes[id];
            return new ItemStack(it, amount);
        }

        private ulong Pack(ItemStack item)
        {
            int id = ItemTypeManager.ItemTypes.IndexOf(item.Item);
            int amount = item.Amount;

            return ((ulong)(uint)id << 32) | (uint)amount;
        }
    }
}