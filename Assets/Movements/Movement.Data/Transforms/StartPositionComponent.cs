using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data.Transforms.StartEnd
{
    [Serializable]
    public struct StartPositionComponent : IComponentData
    {
        public float3 value;
    }
}