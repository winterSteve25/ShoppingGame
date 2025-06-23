using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Items
{
    public static class ItemTypeManager
    {
        public static List<ItemType> ItemTypes { get; private set; }
        public static Lazy<List<ItemType>> CommonItems =
            new(() => ItemTypes.Where(x => x.Rarity == ItemRarity.Common).ToList());
        public static Lazy<List<ItemType>> UncommonItems =
            new(() => ItemTypes.Where(x => x.Rarity == ItemRarity.Uncommon).ToList());
        public static Lazy<List<ItemType>> RareItems =
            new(() => ItemTypes.Where(x => x.Rarity == ItemRarity.Rare).ToList());

        [RuntimeInitializeOnLoadMethod]
        private static void Collect()
        {
            ItemTypes = Resources.LoadAll<ItemType>("ItemTypes").ToList();
        }
    }
}