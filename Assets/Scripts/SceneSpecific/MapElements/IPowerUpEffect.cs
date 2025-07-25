using System;
using Player;

namespace SceneSpecific.MapElements
{
    public interface IPowerUpEffect
    {
        float EffectDuration { get; }
        
        void Apply(PlayerCharacterController player);
        Action<PlayerCharacterController> RemoveEffect();
    }
}