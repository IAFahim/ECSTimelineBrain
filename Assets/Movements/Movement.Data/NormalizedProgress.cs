using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct NormalizedProgress : IComponentData
    {
        public float value;
    }
}