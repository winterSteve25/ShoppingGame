using Items;

namespace Objective
{
    public interface IItemTaker
    {
        bool Submit(WorldItem itemHeld);
    }
}