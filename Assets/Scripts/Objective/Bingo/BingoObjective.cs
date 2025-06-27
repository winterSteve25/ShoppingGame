using System.Collections.Generic;
using Items;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Objective.Bingo
{
    [DefaultExecutionOrder(100)]
    public class BingoObjective : NetworkBehaviour
    {
        [SerializeField] private Transform[] commonItemSpawns;
        [SerializeField] private Transform[] uncommonItemSpawns;
        [SerializeField] private Transform[] rareItemSpawns;
        [SerializeField] private BingoBoardBehaviour[] bingoBoards;
        [SerializeField] private NetworkObject worldItemPrefab;

        private ItemStack2DArray _board;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                if (_board == null)
                {
                    RequestBoardSyncRpc();
                }

                return;
            }

            GenerateItemPool(out var common, out var uncommon, out var rare);
            SpawnItemsIntoPosition(common, commonItemSpawns);
            SpawnItemsIntoPosition(uncommon, uncommonItemSpawns);
            SpawnItemsIntoPosition(rare, rareItemSpawns);
            var board = GenerateBoard(
                commonItemSpawns.Length,
                uncommonItemSpawns.Length,
                rareItemSpawns.Length,
                common,
                uncommon,
                rare
            );

            foreach (var b in bingoBoards)
            {
                b.SyncBoardRpc(new BingoBoard(board));
            }

            _board = board;
            SyncBoardRpc(_board);
        }

        private void GenerateItemPool(out ItemStack[] common, out ItemStack[] uncommon, out ItemStack[] rare)
        {
            common = GenerateItemPool(ItemTypeManager.CommonItems.Value, commonItemSpawns.Length);
            uncommon = GenerateItemPool(ItemTypeManager.UncommonItems.Value, uncommonItemSpawns.Length);
            rare = GenerateItemPool(ItemTypeManager.RareItems.Value, rareItemSpawns.Length);
        }

        private ItemStack[] GenerateItemPool(List<ItemType> pool, int amount)
        {
            var numberOfTypes = Mathf.Max(2, Mathf.RoundToInt(Mathf.Sqrt(2 * amount - 2) - 1));
            var result = new List<ItemStack>();

            // Ensure we have enough amount to give at least 1 to each type
            if (amount < numberOfTypes)
            {
                numberOfTypes = amount;
            }

            // Start by giving 1 to each type to ensure no zeros
            var remaining = amount - numberOfTypes;
            var amounts = new int[numberOfTypes];
            for (int i = 0; i < numberOfTypes; i++)
            {
                amounts[i] = 1; // Minimum of 1 for each
            }

            // Distribute the remaining amount more evenly
            var baseAmount = remaining / numberOfTypes;
            var extraAmount = remaining % numberOfTypes;

            // Add base amount to all
            for (int i = 0; i < numberOfTypes; i++)
            {
                amounts[i] += baseAmount;
            }

            // Distribute the remainder randomly but with controlled variance
            var maxVariance = Mathf.Max(1, baseAmount / 2); // Limit variance to half of base amount

            for (int i = 0; i < extraAmount; i++)
            {
                var randomIndex = Random.Range(0, numberOfTypes);
                amounts[randomIndex]++;
            }

            // Add some controlled randomness by redistributing small amounts
            for (int i = 0; i < numberOfTypes / 2; i++) // Only do this for half the types to avoid too much shuffling
            {
                var sourceIndex = Random.Range(0, numberOfTypes);
                var targetIndex = Random.Range(0, numberOfTypes);

                if (sourceIndex != targetIndex && amounts[sourceIndex] > 1)
                {
                    var transferAmount = Mathf.Min(maxVariance, amounts[sourceIndex] - 1);
                    transferAmount = Random.Range(1, transferAmount + 1);

                    amounts[sourceIndex] -= transferAmount;
                    amounts[targetIndex] += transferAmount;
                }
            }

            var alreadyExists = new Dictionary<int, int>();

            // Create the result with random item types
            for (int i = 0; i < numberOfTypes; i++)
            {
                var indexItem = Random.Range(0, pool.Count);
                if (alreadyExists.ContainsKey(indexItem))
                {
                    indexItem = Random.Range(0, pool.Count);
                    if (alreadyExists.TryGetValue(indexItem, out var alreadyIndex))
                    {
                        var itemStack = result[alreadyIndex];
                        itemStack.Amount += amounts[i];
                        result[alreadyIndex] = itemStack;
                    }
                    else
                    {
                        alreadyExists.Add(indexItem, result.Count);
                        result.Add(new ItemStack(pool[indexItem], amounts[i]));
                    }
                }
                else
                {
                    alreadyExists.Add(indexItem, result.Count);
                    result.Add(new ItemStack(pool[indexItem], amounts[i]));
                }
            }

            return result.ToArray();
        }

        private void SpawnItemsIntoPosition(ItemStack[] items, Transform[] positions)
        {
            int index = 0;
            int amount = items[index].Amount;

            foreach (var pos in positions)
            {
                if (amount <= 0)
                {
                    index++;
                    amount = items[index].Amount;
                }

                amount--;
                var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(worldItemPrefab, destroyWithScene: true,
                    position: pos.position);
                if (obj.TryGetComponent(out WorldItem worldItem))
                {
                    worldItem.ItemType = items[index].Item;
                }
            }
        }

        private ItemStack2DArray GenerateBoard(
            int commonAmount,
            int uncommonAmount,
            int rareAmount,
            ItemStack[] commonItems,
            ItemStack[] uncommonItems,
            ItemStack[] rareItems)
        {
            // Count total available items by summing all amounts
            var allAvailableItems = new List<ItemStack>();
            allAvailableItems.AddRange(commonItems);
            allAvailableItems.AddRange(uncommonItems);
            allAvailableItems.AddRange(rareItems);

            var totalAvailableItems = 0;
            foreach (var itemStack in allAvailableItems)
            {
                totalAvailableItems += itemStack.Amount;
            }

            // Determine appropriate dimension based on reasonable bingo goal amounts
            // Assume each bingo slot should require a reasonable amount of items (e.g., 5-20 items average)
            var averageItemsPerSlot = 2; // Adjust this value based on desired difficulty
            var reasonableSlotCount = totalAvailableItems / averageItemsPerSlot;

            // Determine appropriate dimension based on reasonable slot count
            var calculatedDimension = Mathf.FloorToInt(Mathf.Sqrt(reasonableSlotCount));

            // Ensure minimum dimension of 3 and maximum reasonable dimension (e.g., 7)
            calculatedDimension = Mathf.Clamp(calculatedDimension, 3, 7);

            // If we don't have enough items for even a 3x3 board, we'll need to repeat items
            var actualDimension = calculatedDimension;

            var board = new ItemStack2DArray(actualDimension);
            var totalAmountItems = commonAmount + uncommonAmount + rareAmount;
            var proportionCommon = (float)commonAmount / totalAmountItems;
            var proportionUncommon = (float)uncommonAmount / totalAmountItems;
            var proportionRare = (float)rareAmount / totalAmountItems;
            var numSlots = actualDimension * actualDimension;
            var numCommonSlots = Mathf.CeilToInt(proportionCommon * numSlots);
            var numUncommonSlots = Mathf.CeilToInt(proportionUncommon * numSlots);
            var numRareSlots = Mathf.CeilToInt(proportionRare * numSlots);

            // Adjust slot counts to ensure they don't exceed total slots
            var totalCalculatedSlots = numCommonSlots + numUncommonSlots + numRareSlots;
            if (totalCalculatedSlots > numSlots)
            {
                // Reduce slots proportionally, prioritizing rare items
                var excess = totalCalculatedSlots - numSlots;
                numCommonSlots = Mathf.Max(0, numCommonSlots - excess);
                excess -= (numCommonSlots + numUncommonSlots + numRareSlots) - numSlots;
                if (excess > 0)
                {
                    numUncommonSlots = Mathf.Max(0, numUncommonSlots - excess);
                }
            }
            else if (totalCalculatedSlots < numSlots)
            {
                // Fill remaining slots with common items
                numCommonSlots += numSlots - totalCalculatedSlots;
            }

            // Create a list to hold all selected items for the board
            var boardItems = new List<ItemStack>();

            // Add common items
            for (int i = 0; i < numCommonSlots; i++)
            {
                if (commonItems.Length > 0)
                {
                    var randomItem = commonItems[UnityEngine.Random.Range(0, commonItems.Length)];
                    // Create a reasonable bingo goal for common items (10-50% of available)
                    var minGoal = Mathf.Max(1, randomItem.Amount / 10);
                    var maxGoal = Mathf.Max(minGoal + 1, randomItem.Amount / 2);
                    var goalAmount = UnityEngine.Random.Range(minGoal, maxGoal + 1);
                    boardItems.Add(new ItemStack(randomItem.Item, goalAmount));
                }
            }

            // Add uncommon items
            for (int i = 0; i < numUncommonSlots; i++)
            {
                if (uncommonItems.Length > 0)
                {
                    var randomItem = uncommonItems[UnityEngine.Random.Range(0, uncommonItems.Length)];
                    // Create a reasonable bingo goal for uncommon items (5-30% of available)
                    var minGoal = Mathf.Max(1, randomItem.Amount / 20);
                    var maxGoal = Mathf.Max(minGoal + 1, randomItem.Amount / 3);
                    var goalAmount = UnityEngine.Random.Range(minGoal, maxGoal + 1);
                    boardItems.Add(new ItemStack(randomItem.Item, goalAmount));
                }
            }

            // Add rare items
            for (int i = 0; i < numRareSlots; i++)
            {
                if (rareItems.Length > 0)
                {
                    var randomItem = rareItems[UnityEngine.Random.Range(0, rareItems.Length)];
                    // Create a reasonable bingo goal for rare items (1-20% of available)
                    var minGoal = 1;
                    var maxGoal = Mathf.Max(2, randomItem.Amount / 5);
                    var goalAmount = UnityEngine.Random.Range(minGoal, maxGoal + 1);
                    boardItems.Add(new ItemStack(randomItem.Item, goalAmount));
                }
            }

            // Ensure we have enough items to fill the entire board
            while (boardItems.Count < numSlots)
            {
                if (allAvailableItems.Count > 0)
                {
                    var randomItem = allAvailableItems[UnityEngine.Random.Range(0, allAvailableItems.Count)];
                    // Use moderate goal amounts for fill-in items
                    var minGoal = Mathf.Max(1, randomItem.Amount / 15);
                    var maxGoal = Mathf.Max(minGoal + 1, randomItem.Amount / 4);
                    var goalAmount = UnityEngine.Random.Range(minGoal, maxGoal + 1);
                    boardItems.Add(new ItemStack(randomItem.Item, goalAmount));
                }
                else
                {
                    // Fallback: if no items available, break to avoid infinite loop
                    break;
                }
            }

            // Shuffle the board items to randomize their positions
            for (int i = 0; i < boardItems.Count; i++)
            {
                var temp = boardItems[i];
                var randomIndex = UnityEngine.Random.Range(i, boardItems.Count);
                boardItems[i] = boardItems[randomIndex];
                boardItems[randomIndex] = temp;
            }

            // Fill the board with the shuffled items
            int itemIndex = 0;
            for (int y = 0; y < actualDimension; y++)
            {
                for (int x = 0; x < actualDimension; x++)
                {
                    board[x, y] = boardItems[itemIndex];
                    itemIndex++;
                }
            }

            return board;
        }

        [Rpc(SendTo.Server)]
        private void RequestBoardSyncRpc(RpcParams sender = default)
        {
            SyncBoardToRpc(_board, RpcTarget.Single(sender.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SyncBoardToRpc(ItemStack2DArray board, RpcParams _)
        {
            _board = board;

            foreach (var b in bingoBoards)
            {
                b.SyncBoardRpc(new BingoBoard(board));
            }
        }

        [Rpc(SendTo.NotServer)]
        private void SyncBoardRpc(ItemStack2DArray board)
        {
            _board = board;
        }
    }
}