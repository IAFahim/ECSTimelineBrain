using Rukhanka;
using Unity.Entities;
using Unity.Mathematics;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Data
{
    /// <summary>
    /// Represents a single flat animation contribution.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct BlendGroupEntry : IBufferElementData
    {
        public int LayerIndex;
        public Hash128 ClipHash;
        public float NormalizedTime;
        public float Weight;
        public Hash128 AvatarMaskHash;
        public AnimationBlendingMode BlendMode;
    }

    /// <summary>
    /// Stores the last active frame's timeline entries.
    /// Used to smoothly fade out animations after the timeline has stopped.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct PreviousBlendGroupEntry : IBufferElementData
    {
        public int LayerIndex;
        public Hash128 ClipHash;
        public float NormalizedTime;
        public float Weight;
        public Hash128 AvatarMaskHash;
        public AnimationBlendingMode BlendMode;
    }

    public struct BlendGroupTimer : IComponentData, IEnableableComponent
    {
        public float TimelineWeight; // 0.0 = Fully Fallback, 1.0 = Fully Timeline
        public float FallbackAccumulatedTime;
    }

    public struct BlendGroupFallBackForNoAnimationToProcessComponent : IComponentData
    {
        public Hash128 ClipHash; 
        public float BlendInSpeed;  // 1.0f / BlendInDuration
        public float BlendOutSpeed; // 1.0f / BlendOutDuration
    }

    /// <summary>
    /// Attached to a Track Entity. Overrides the target's fallback animation when the track plays.
    /// </summary>
    public struct TrackFallbackOverride : IComponentData
    {
        public Hash128 FallbackClipHash;
        public float BlendInSpeed;
        public float BlendOutSpeed;
    }
}