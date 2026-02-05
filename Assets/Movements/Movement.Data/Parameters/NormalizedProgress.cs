using System;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Timing
{
    [Serializable]
    public struct NormalizedProgress : IComponentData
    {
        public float value;
    }
}