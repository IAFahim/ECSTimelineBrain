using Rukhanka;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
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


    public class BlendGroupEntryAuthoring : MonoBehaviour
    {
        public AnimationClip fallbackAnimationClip;
        public float blendInDuration; 
        public float blendOutDuration; 
        
        public class BlendGroupEntryBaker : Baker<BlendGroupEntryAuthoring>
        {
            public override void Bake(BlendGroupEntryAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<BlendGroupTimer>(entity);
                AddComponent(entity, new BlendGroupFallBackForNoAnimationToProcessComponent
                {
                    // TODO:
                });
                
                AddBuffer<BlendGroupEntry>(entity);
            }
        }
    }
}