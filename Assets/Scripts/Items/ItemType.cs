using System;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item Type", menuName = "Item Type")]
    public class ItemType : ScriptableObject
    {
        [field: SerializeField] public ItemRarity Rarity { get; private set; }
    }

    [Serializable]
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare
    }
}