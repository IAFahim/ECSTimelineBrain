using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using Unity.Properties;

namespace Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data.Targets
{
    public struct TargetAnimationComponent : IAnimatedComponent<Target>
    {
        [CreateProperty] public Target Value { get; set; }
    }
}