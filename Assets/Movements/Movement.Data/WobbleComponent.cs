using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct WobbleComponent : IComponentData
    {
        public float value;
    }
}