using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct HeightComponent : IComponentData
    {
        public float value;
    }
}