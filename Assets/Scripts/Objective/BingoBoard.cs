using Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Objective
{
    public class BingoBoard : NetworkBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;

        [SerializeField] private RectTransform canvas;
        [SerializeField] private GridLayoutGroup parent;
        [SerializeField] private BingoCell cellPrefab;

        private ItemStack2DArray _board;

        public override void OnDestroy()
        {
            if (_board == null) return;
            _board.Dispose();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SyncBoardRpc(ItemStack2DArray arr)
        {
            if (_board != null) return;
            _board = arr;
            RecreateBoard();
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SyncBoardToRpc(ItemStack2DArray arr, RpcParams _)
        {
            _board = arr;
            RecreateBoard();
        }

        [Rpc(SendTo.Server)]
        private void RequestBoardSyncRpc(RpcParams param = default)
        {
            if (_board == null)
            {
                Debug.LogError("Can not sync bingo board because it hasn't been setup.");
                return;
            }
            
            SyncBoardToRpc(_board, RpcTarget.Single(param.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        private void RecreateBoard()
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Destroy(parent.transform.GetChild(i).gameObject);
            }

            parent.cellSize = new Vector2(canvas.sizeDelta.x / width - parent.spacing.x, canvas.sizeDelta.y / height - parent.spacing.y);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var bingoCell = Instantiate(cellPrefab, parent.transform);
                    bingoCell.Init(_board[i, j]);
                }
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) parent.transform);
        }

        public void SubmitItem(WorldItem item)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
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

            Debug.Log("Submitted unneeded item");

            end: ;
            Debug.Log("Submitted item");
        }

        public void RemoveItem(WorldItem item)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
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
            var itemStack = _board[i, j];
            itemStack.Amount--;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RemoveItemRpc(int i, int j)
        {
            var itemStack = _board[i, j];
            itemStack.Amount++;
        }
    }
}