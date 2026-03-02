using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PositionResetOnDeactivate : IComponentData
    {
        public float3 Value;
    }
}