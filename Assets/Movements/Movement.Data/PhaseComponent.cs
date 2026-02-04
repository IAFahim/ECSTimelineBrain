using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct PhaseComponent : IComponentData
    {
        public float value;
    }
}