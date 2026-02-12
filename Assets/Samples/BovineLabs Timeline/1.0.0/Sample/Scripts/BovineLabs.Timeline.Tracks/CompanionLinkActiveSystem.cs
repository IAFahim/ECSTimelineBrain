using Unity.Entities;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct CompanionLinkActiveSystem : ISystem
    {
    }
}