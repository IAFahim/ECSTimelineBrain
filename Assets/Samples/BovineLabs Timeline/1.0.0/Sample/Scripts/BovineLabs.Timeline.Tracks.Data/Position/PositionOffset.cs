using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public enum OffsetType : byte
    {
        World,
        Local
    }

    public struct PositionOffset : IComponentData
    {
        public OffsetType Type;
        public float3 Offset;
    }
}