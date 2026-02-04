using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct SpeedComponent : IComponentData
    {
        public float value;
    }
}