using System;
using Unity.Entities;
using UnityEngine;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct TargetTranfromComponent : IComponentData
    {
        public UnityObjectRef<Transform> value;
    }
}