using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data.Animations
{
    public struct RukhankaAnimationClipData : IComponentData
    {
        public Hash128 AnimationHash;
        public float Duration;
    }
}