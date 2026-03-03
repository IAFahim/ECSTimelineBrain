using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data.Animations
{
    public struct BlendTreePlaybackState : IComponentData
    {
        public float AccumulatedTime;
        public float PreviousAbsoluteTime;
        public bool IsInitialized;
    }
}