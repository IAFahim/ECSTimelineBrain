using System;
using AV.Eases.Runtime;
using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Easing
{
    [Serializable]
    public struct MoveEaseComponent : IComponentData
    {
        public EaseConfig value;
    }
}