using Unity.Entities;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Tracks.Data.Animations
{
    public struct IdleAnimationClip : IComponentData
    {
        public Hash128 AnimationHash;
    }
}