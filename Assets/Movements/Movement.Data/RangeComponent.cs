using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct RangeComponent : IComponentData
    {
        public float value;
    }
}
