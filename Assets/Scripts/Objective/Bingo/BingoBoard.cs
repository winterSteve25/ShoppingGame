using Items;
using Unity.Netcode;
using Utils;

namespace Objective.Bingo
{
    public class BingoBoard : INetworkSerializable
    {
        public int Dimension => _board.Dimension;
        private ItemStack2DArray _board;
        private int[] _completionProgress;

        public BingoBoard()
        {
        }

        public BingoBoard(ItemStack2DArray board)
        {
            _board = board;
            _completionProgress = new int[board.Dimension * board.Dimension];
        }

        public bool IsCellComplete(int x, int y)
        {
            return _completionProgress[_board.Dimension * y + x] >= _board[x, y].Amount;
        }

        public ItemStack GetItemProgress(int x, int y)
        {
            return new ItemStack(_board[x, y].Item, _completionProgress[_board.Dimension * y + x]);
        }

        public ItemStack GetItemNeeded(int x, int y)
        {
            return _board[x, y];
        } 

        public ItemType GetItemType(int x, int y)
        {
            return _board[x, y].Item;
        }

        public void IncrementItemProgress(int x, int y)
        {
            _completionProgress[_board.Dimension * y + x]++;
        } 
        
        public void DecrementItemProgress(int x, int y)
        {
            _completionProgress[_board.Dimension * y + x]--;
        } 
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeNetworkSerializable(ref _board);
            serializer.SerializeValue(ref _completionProgress);
        }
    }
}