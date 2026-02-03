using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct StartQuaternionComponent : IComponentData
    {
        public quaternion value;
    }
}