using System;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item Type", menuName = "Item Type")]
    public class ItemType : ScriptableObject
    {
        [field: SerializeField] public ItemRarity Rarity { get; private set; }
        [field: SerializeField] public Vector3 Scale { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public Mesh Mesh { get; private set; }
        [field: SerializeField] public Material[] Materials { get; private set; }
    }

    [Serializable]
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare
    }
}