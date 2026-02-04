using System;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Timing
{
    [Serializable]
    public struct PhaseComponent : IComponentData
    {
        public float value;
    }
}