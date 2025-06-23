using System.Collections.Generic;
using System.Linq;
using Items;
using Unity.Netcode;
using UnityEngine;
using Utils;
using Random = System.Random;

namespace Objective
{
    [DefaultExecutionOrder(100)]
    public class BingoObjectiveManager : NetworkBehaviour
    {
        [SerializeField] private BingoBoard[] bingoBoards;
        [SerializeField] private Transform[] commonItemSpawns;
        [SerializeField] private Transform[] uncommonItemSpawns;
        [SerializeField] private Transform[] rareItemSpawns;
        [SerializeField] private WorldItem worldItemPrefab;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            var boards = GenerateBoard();

            for (int i = 0; i < bingoBoards.Length; i++)
            {
                bingoBoards[i].SyncBoardRpc(boards[i]);
            }
        }

        private ItemStack[] SpawnItems()
        {
            var common = SpawnItems(ItemTypeManager.CommonItems.Value, commonItemSpawns.Length);
            var uncommon = SpawnItems(ItemTypeManager.UncommonItems.Value, uncommonItemSpawns.Length);
            var rare = SpawnItems(ItemTypeManager.RareItems.Value, rareItemSpawns.Length);

            return common
                .Concat(uncommon)
                .Concat(rare)
                .ToArray();
        }

        private ItemStack[] SpawnItems(List<ItemType> pool, int amount)
        {
            var numberOfTypes = Mathf.RoundToInt(Mathf.Sqrt(2 * amount - 2) - 1);
            var result = new ItemStack[numberOfTypes];
            var rng = new Random();
            
            int total = amount;
            int avg = Mathf.CeilToInt((float)total / numberOfTypes);
            int maxPiece = Mathf.Min(total, avg * 2);

            for (int i = 0; i < numberOfTypes - 1; i++)
            {
                int remainingParts = numberOfTypes - i - 1;

                // Ensure there's enough left for the remaining parts
                int minForThisPiece = Mathf.Max(0, total - remainingParts * maxPiece);
                int maxForThisPiece = Mathf.Min(maxPiece, total);

                int part = rng.Next(minForThisPiece, maxForThisPiece + 1); // +1 because upper bound is exclusive
                total -= part;

                result[i] = new ItemStack(pool[rng.Next(pool.Count)], part);
            }

            result[numberOfTypes - 1] = new ItemStack(pool[rng.Next(pool.Count)], total);
            return result;
        }

        private ItemStack2DArray[] GenerateBoard()
        {
            return null;
        }
    }
}