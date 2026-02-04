using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct PitchComponent : IComponentData
    {
        public float value;
    }
}