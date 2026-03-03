using BovineLabs.Timeline.Data;
using Rukhanka;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Tracks.Data.Animations
{
    [InternalBufferCapacity(0)]
    public struct BlendTree2DMotionData : IBufferElementData
    {
        public Hash128 AnimationHash;
        public ScriptedAnimator.BlendTree2DMotionElement BlendTree2DMotionElement;
    }
    
    public struct BlendTree2DClipData : IAnimatedComponent<float2>
    {
        [CreateProperty] public float2 Value { get; set; }
    }

    public struct BlendAnimationTree2DTrackData : IComponentData
    {
        public MotionBlob.Type BlendTreeType;
    }
}