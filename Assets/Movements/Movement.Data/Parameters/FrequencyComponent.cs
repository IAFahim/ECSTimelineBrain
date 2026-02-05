using System;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Motion
{
    [Serializable]
    public struct FrequencyComponent : IComponentData
    {
        public float value;
    }
}