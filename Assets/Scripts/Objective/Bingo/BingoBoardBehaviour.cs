using Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Objective.Bingo
{
    public class BingoBoardBehaviour : NetworkBehaviour
    {
        [SerializeField] private RectTransform canvas;
        [SerializeField] private GridLayoutGroup parent;
        [SerializeField] private BingoCell cellPrefab;

        private BingoBoard _board;
        private BingoCell[,] _uiCells;
        
        public void SubmitItem(WorldItem item)
        {
            for (int i = 0; i < _board.Dimension; i++)
            {
                for (int j = 0; j < _board.Dimension; j++)
                {
                    if (_board.IsCellComplete(i, j)) continue;
                    
                    var type = _board.GetItemType(i, j);
                    if (type != item.ItemType) continue;

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
            for (int i = 0; i < _board.Dimension; i++)
            {
                for (int j = 0; j < _board.Dimension; j++)
                {
                    var type = _board.GetItemType(i, j);
                    if (type != item.ItemType)
                    {
                        continue;
                    }

                    RemoveItemRpc(i, j);
                }
            }
        }
        
        private void RecreateBoard()
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Destroy(parent.transform.GetChild(i).gameObject);
            }

            parent.cellSize = new Vector2(canvas.sizeDelta.x / _board.Dimension - parent.spacing.x, canvas.sizeDelta.y / _board.Dimension - parent.spacing.y);
            _uiCells = new BingoCell[_board.Dimension, _board.Dimension];

            for (int i = 0; i < _board.Dimension; i++)
            {
                for (int j = 0; j < _board.Dimension; j++)
                {
                    var bingoCell = Instantiate(cellPrefab, parent.transform);
                    bingoCell.Init(_board.GetItemProgress(i, j).Amount, _board.GetItemNeeded(i, j));
                    _uiCells[i, j] = bingoCell;
                }
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) parent.transform);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void SyncBoardRpc(BingoBoard arr)
        {
            if (_board != null) return;
            _board = arr;
            RecreateBoard();
        }
        
        public void SyncBoard(BingoBoard arr)
        {
            if (_board != null) return;
            _board = arr;
            RecreateBoard();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SubmitItemRpc(int i, int j)
        {
            _board.IncrementItemProgress(i, j);
            _uiCells[i, j].Modify(1);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RemoveItemRpc(int i, int j)
        {
            _board.DecrementItemProgress(i, j);
            _uiCells[i, j].Modify(-1);
        }
    }
}