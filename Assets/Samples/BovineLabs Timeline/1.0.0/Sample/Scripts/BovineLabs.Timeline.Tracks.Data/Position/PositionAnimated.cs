using BovineLabs.Timeline.Data;
using Unity.Mathematics;
using Unity.Properties;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PositionAnimated : IAnimatedComponent<float3>
    {
        [CreateProperty] public float3 Value { get; set; }
    }
}