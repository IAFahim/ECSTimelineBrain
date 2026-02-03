using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct EndQuaternionComponent : IComponentData
    {
        public quaternion value;
    }
}