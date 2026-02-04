using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data.Advanced.Utilities
{
    public struct PreCalculatedPositionsFloat3 : IComponentData
    {
        public BlobAssetReference<Waypoints> blob;
    }

    [Serializable]
    public struct Waypoints
    {
        public BlobArray<float3> values;
    }
}