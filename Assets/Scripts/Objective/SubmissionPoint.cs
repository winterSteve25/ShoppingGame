using Items;
using Unity.Netcode;
using UnityEngine;

namespace Objective
{
    public class SubmissionPoint : NetworkBehaviour
    {
        [SerializeField] private NetworkInventory inventory;
        [SerializeField] private BingoBoard bingoBoard;
        [SerializeField] private int team;
        
        public void AddItem(WorldItem item)
        {
            item.PickUp(inventory, transform);
            bingoBoard.SubmitItem(item);
        }

        public void RemoveItem(WorldItem item)
        {
            item.Drop();
            bingoBoard.RemoveItem(item);
        }
    }
}