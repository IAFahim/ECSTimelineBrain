using System;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Motion
{
    [Serializable]
    public struct PitchComponent : IComponentData
    {
        public float value;
    }
}