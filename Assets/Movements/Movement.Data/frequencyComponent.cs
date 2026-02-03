using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct FrequencyComponent : IComponentData
    {
        public float value;
    }
}