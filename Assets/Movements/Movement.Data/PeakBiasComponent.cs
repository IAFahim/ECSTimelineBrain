using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct PeakBiasComponent : IComponentData
    {
        public float value;
    }
}