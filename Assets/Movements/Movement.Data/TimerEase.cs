using System;
using AV.Eases.Runtime;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct TimerEase : IComponentData
    {
        public EaseConfig value;
    }
}