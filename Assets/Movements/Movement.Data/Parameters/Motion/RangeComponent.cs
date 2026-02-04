using System;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Motion
{
    [Serializable]
    public struct RangeComponent : IComponentData
    {
        public float value;
    }
}