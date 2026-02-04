using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data
{
    /// <summary>
    /// Mainly for storing the calculated value for class based Transform 
    /// </summary>
    [Serializable]
    public struct UnAssignedQuaternionComponent : IComponentData
    {
        public quaternion value;
    }
}