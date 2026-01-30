using System;
using GameVariable.Intent;
using Unity.Entities;

namespace Intents.Intent.Data
{
    [Serializable]
    public struct IntentComponent : IComponentData
    {
        public IntentState value;
    }
}