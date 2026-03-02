using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PositionTarget : IComponentData
    {
        public Entity Target;
        public float3 Offset;
        public OffsetType Type;
    }
}