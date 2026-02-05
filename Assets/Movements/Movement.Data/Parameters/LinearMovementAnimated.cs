using BovineLabs.Timeline.Data;
using Unity.Mathematics;
using Unity.Properties;

namespace Movements.Movement.Data.Parameters
{
    
    public struct LinearMovementAnimated : IAnimatedComponent<float3>
    {
        
        [CreateProperty]
        public float3 Value { get; set; }
    }

    
    public struct LinearMovementRotationAnimated : IAnimatedComponent<quaternion>
    {
        [CreateProperty]
        public quaternion Value { get; set; }
    }
}