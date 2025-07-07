using Items;

namespace Objective
{
    public interface IItemTaker
    {
        float TimeToSubmit { get; }
        
        bool Submit(WorldItem itemHeld);
    }
}