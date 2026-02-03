using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct EndPositionComponent : IComponentData
    {
        public float3 value;
    }
}