using System;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Easing
{
    [Serializable]
    public struct PeakBiasComponent : IComponentData
    {
        public float value;
    }
}