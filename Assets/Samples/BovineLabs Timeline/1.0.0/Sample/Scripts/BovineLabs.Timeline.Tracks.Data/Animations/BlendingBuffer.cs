using Rukhanka;
using Unity.Entities;
using Unity.Mathematics;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Data
{
    /// <summary>
    /// Represents a single flat animation contribution.
    /// Track systems (Single, 1D, 2D) decompose their logic and write raw clips here.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct BlendGroupEntry : IBufferElementData
    {
        /// <summary>
        /// Grouping ID. Typically maps to the Timeline Track / Layer Index.
        /// Entries in the same group are summed and normalized together.
        /// </summary>
        public int LayerIndex;

        /// <summary>
        /// Unique animation clip identifier.
        /// </summary>
        public Hash128 ClipHash;

        /// <summary>
        /// The local normalized time (0.0 to 1.0) for this specific clip.
        /// </summary>
        public float NormalizedTime;

        /// <summary>
        /// The raw weight assigned by the timeline/blend tree before layer normalization.
        /// </summary>
        public float Weight;

        /// <summary>
        /// (Optional) Avatar mask for partial-body tracks. Default is empty.
        /// </summary>
        public Hash128 AvatarMaskHash;

        /// <summary>
        /// Allows tracks to act as Additive (e.g., breathing, recoil) instead of Override.
        /// </summary>
        public AnimationBlendingMode BlendMode;
    }

    /// <summary>
    /// Tracks the crossfade state between the Timeline and the Fallback animation.
    /// </summary>
    public struct BlendGroupTimer : IComponentData, IEnableableComponent
    {
        /// <summary>
        /// 0.0 = Fully Fallback, 1.0 = Fully Timeline.
        /// </summary>
        public float TimelineWeight;
        
        /// <summary>
        /// Automatically advances the internal time of the fallback animation so it loops smoothly.
        /// </summary>
        public float FallbackAccumulatedTime;
    }

    /// <summary>
    /// Defines the default loop to play when the timeline is inactive or blending in/out.
    /// </summary>
    public struct BlendGroupFallBackForNoAnimationToProcessComponent : IComponentData
    {
        public Hash128 ClipHash; 
        
        // Storing speed instead of duration avoids divisions at runtime.
        // Speed = 1.0f / Duration.
        public float BlendInSpeed;  
        public float BlendOutSpeed; 
    }
}