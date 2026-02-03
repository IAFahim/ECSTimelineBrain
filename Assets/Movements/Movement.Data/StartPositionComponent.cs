using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct StartPositionComponent : IComponentData
    {
        public float3 value;
    }
}