using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item Type", menuName = "Item Type")]
    public class ItemType : ScriptableObject
    {
        [field: SerializeField] public int Min { get; private set; }
        [field: SerializeField] public int Max { get; private set; }
    }
}