using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct DirectionComponent : IComponentData
    {
        public float3 value;
    }
}