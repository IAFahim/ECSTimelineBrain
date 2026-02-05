using BovineLabs.Timeline.Data;
using Unity.Mathematics;
using Unity.Properties;

namespace Movements.Movement.Data.Transforms
{
    
    public struct PositionAnimated : IAnimatedComponent<float3>
    {
        [CreateProperty]
        public float3 Value { get; set; }
    }

    
    public struct QuaternionAnimated : IAnimatedComponent<quaternion>
    {
        [CreateProperty]
        public quaternion Value { get; set; }
    }
}