using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct AmplitudeComponent : IComponentData
    {
        public float value;
    }
}