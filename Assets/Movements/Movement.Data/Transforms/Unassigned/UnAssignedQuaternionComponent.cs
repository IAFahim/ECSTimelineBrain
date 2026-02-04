using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data.Transforms.Unassigned
{
    /// <summary>
    ///     Mainly for storing the calculated value for class based Transform
    /// </summary>
    [Serializable]
    public struct UnAssignedQuaternionComponent : IComponentData
    {
        public quaternion value;
    }
}