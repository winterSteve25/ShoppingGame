using Items;
using Unity.Netcode;
using UnityEngine;

namespace Objective
{
    public class SubmissionPoint : NetworkBehaviour, IItemTaker
    {
        [SerializeField] private NetworkInventory inventory;
        [SerializeField] private BingoBoard bingoBoard;
        [SerializeField] private byte team;
        [SerializeField] private Transform[] points;
        
        public bool Submit(WorldItem itemHeld)
        {
            itemHeld.Drop();
            itemHeld.SetAnchor(points[Random.Range(0, points.Length)]);
            itemHeld.OnPicked += OnItemPicked;
            
            inventory.AddItem(itemHeld);
            bingoBoard.SubmitItem(itemHeld);
            
            return true;
        }

        private void OnItemPicked(WorldItem item)
        {
            item.OnPicked -= OnItemPicked;
            bingoBoard.RemoveItem(item);
            inventory.RemoveItem(item);
        }
    }
}