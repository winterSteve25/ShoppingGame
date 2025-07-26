using System;
using Player;

namespace SceneSpecific.MapElements
{
    public interface IPowerUpEffect
    {
        void Apply(PlayerCharacterController player);
        Action<PlayerCharacterController> RemoveEffect();
    }
}