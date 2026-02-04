using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data.Transforms.StartEnd
{
    [Serializable]
    public struct StartQuaternionComponent : IComponentData
    {
        public quaternion value;
    }
}