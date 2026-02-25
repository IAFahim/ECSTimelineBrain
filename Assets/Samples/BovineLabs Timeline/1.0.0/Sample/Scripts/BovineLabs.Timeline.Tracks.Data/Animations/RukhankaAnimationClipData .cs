using BovineLabs.Timeline.Data;
using Unity.Entities;
using Unity.Properties;

namespace BovineLabs.Timeline.Tracks.Data.Animations
{
    public struct RukhankaAnimationClipAnimated : IAnimatedComponent<float>
    {
        public Hash128 AnimationHash;
        [CreateProperty] public float Value { set; get; }
    }
}