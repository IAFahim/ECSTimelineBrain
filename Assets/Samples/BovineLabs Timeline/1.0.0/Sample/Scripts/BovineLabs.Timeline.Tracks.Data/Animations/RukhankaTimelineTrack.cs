using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data.Animations
{
    public struct RukhankaTimelineTrack : IComponentData
    {
        public Hash128 ExitIdleClipHash;
        public float ExitTransitionDuration;
    }
}