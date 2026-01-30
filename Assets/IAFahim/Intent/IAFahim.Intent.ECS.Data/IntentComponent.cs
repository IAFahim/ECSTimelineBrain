using System;
using GameVariable.Intent;
using Unity.Entities;

namespace IAFahim.Intent.ECS.Data
{
    [Serializable]
    public struct IntentComponent : IComponentData
    {
        public IntentState value;
    }
}