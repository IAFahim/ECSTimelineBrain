using System;
using AV.Eases.Runtime;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct TimerEaseComponent : IComponentData
    {
        public EaseConfig value;
    }
}