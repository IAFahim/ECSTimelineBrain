using Unity.Entities;

namespace SpatialMaps.SpatialMaps.Data
{
    public struct Neighbours : IBufferElementData
    {
        public Entity Entity;
    }
}