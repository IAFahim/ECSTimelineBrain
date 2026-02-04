using System;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Motion
{
    [Serializable]
    public struct HeightComponent : IComponentData
    {
        public float value;
    }
}