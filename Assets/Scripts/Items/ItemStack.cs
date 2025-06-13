namespace Items
{
    public struct ItemStack
    {
        public ItemType Item;
        public int Amount;

        public ItemStack(ItemType item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}