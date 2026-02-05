using System;
using Unity.Entities;

namespace Movements.Movement.Data.Tags.Modifiers
{
    
    /// <summary>
    /// -1.0 for reverse while calculating if enabled
    /// </summary>
    [Serializable]
    public struct ReverseDirectionEnableableComponent : IComponentData, IEnableableComponent 
    {
    }
}