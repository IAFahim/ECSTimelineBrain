using Rukhanka;
using Unity.Entities;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Data
{
    /// <summary>
    /// Represents a single animation contribution within a blend group.
    /// All entries sharing the same BlendGroupId form a normalized blend set.
    /// </summary>
    public struct BlendGroupEntry : IBufferElementData
    {
        /// <summary>
        /// Identifier for the blend group this entry belongs to.
        /// </summary>
        public byte BlendGroupId;

        /// <summary>
        /// The motion category for this animation entry.
        /// </summary>
        public MotionBlob.Type MotionType;

        /// <summary>
        /// Unique animation clip identifier.
        /// </summary>
        public Hash128 ClipHash;

        /// <summary>
        /// Normalized weight within its blend group (group sum = 1).
        /// </summary>
        public float NormalizedWeight;
    }

    public struct BlendGroupTimer : IComponentData, IEnableableComponent
    {
        public float CurrentDurationToLatest;
        public float MaxDurationToLatest;
    }

    /// <summary>
    /// Fall Back for no clip in AnimationToProcessComponent so that it doesn't freeze
    /// Self loop
    /// </summary>
    public struct BlendGroupFallBackForNoAnimationToProcessComponent : IComponentData
    {
        public Hash128 ClipHash; 
        public float BlendInDuration; 
        public float BlendOutDuration; 
    }
}