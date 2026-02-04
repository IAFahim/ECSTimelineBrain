using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct PreCalculatedPositionFloat3Waypoint : IComponentData
    {
        public float3[] values;
    }
}