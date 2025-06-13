using Unity.Netcode;

namespace Objective
{
    public interface IInteractable
    {
        bool Interact(NetworkObject player);
    }
}