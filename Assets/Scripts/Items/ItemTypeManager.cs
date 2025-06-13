using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Items
{
    public static class ItemTypeManager
    {
        public static List<ItemType> ItemTypes { get; private set; }
        
        [RuntimeInitializeOnLoadMethod]
        private static void Collect()
        {
            ItemTypes = Resources.LoadAll<ItemType>("ItemTypes").ToList();
        }
    }
}