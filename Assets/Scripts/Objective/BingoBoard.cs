using Items;
using Unity.Netcode;

namespace Objective
{
    public class BingoBoard : NetworkBehaviour
    {
        // The amount represents the amount left to fulfill
        // Synchronized via RPC calls
        private ItemStack[,] _board;

        public void SubmitItem(WorldItem item)
        {
            for (int i = 0; i < _board.GetLength(0); i++)
            {
                for (int j = 0; j < _board.GetLength(1); j++)
                {
                    var itemStack = _board[i, j];
                    if (itemStack.Item != item.ItemType || itemStack.Amount <= 0)
                    {
                        continue;
                    }

                    SubmitItemRpc(i, j);
                    goto end;
                }
            }
            
            // handle submitted unneeded item
            
            end: ;
        }

        public void RemoveItem(WorldItem item)
        {
            for (int i = 0; i < _board.GetLength(0); i++)
            {
                for (int j = 0; j < _board.GetLength(1); j++)
                {
                    var itemStack = _board[i, j];
                    if (itemStack.Item != item.ItemType)
                    {
                        continue;
                    }
                    
                    RemoveItemRpc(i, j);
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SubmitItemRpc(int i, int j)
        {
            _board[i, j].Amount--;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RemoveItemRpc(int i, int j)
        {
            _board[i, j].Amount++;
        }
    }
}