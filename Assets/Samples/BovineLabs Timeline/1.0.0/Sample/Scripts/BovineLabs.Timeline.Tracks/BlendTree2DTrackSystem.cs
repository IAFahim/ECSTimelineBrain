using Rukhanka;
using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Systems
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateBefore(typeof(AnimationProcessSystem))]
    public partial struct BlendTree2DTrackSystem : ISystem
    {
        
    }
}