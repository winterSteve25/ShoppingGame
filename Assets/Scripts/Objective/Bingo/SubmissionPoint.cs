using Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objective.Bingo
{
    public class SubmissionPoint : NetworkBehaviour, IItemTaker
    {
        [SerializeField] private NetworkInventory inventory;
        [FormerlySerializedAs("bingoBoard")] [SerializeField] private BingoBoardBehaviour bingoBoardBehaviour;
        [SerializeField] private byte team;
        [SerializeField] private Transform[] points;

        public float TimeToSubmit => 2f;

        public bool Submit(WorldItem itemHeld)
        {
            itemHeld.Drop();
            itemHeld.SetAnchor(points[Random.Range(0, points.Length)]);
            
            SubmitOnServerRpc(itemHeld);
            return true;
        }

        [Rpc(SendTo.Server)]
        private void SubmitOnServerRpc(NetworkBehaviourReference worldItem)
        {
            if (!worldItem.TryGet(out WorldItem itemHeld)) return;
            itemHeld.OnPicked += OnItemPicked;
            itemHeld.IsOnTree = true;
            inventory.AddItem(itemHeld);
            bingoBoardBehaviour.SubmitItem(itemHeld);
        }

        private void OnItemPicked(WorldItem item)
        {
            item.OnPicked -= OnItemPicked;
            item.IsOnTree = false;
            bingoBoardBehaviour.RemoveItem(item);
            inventory.RemoveItem(item);
        }
    }
}