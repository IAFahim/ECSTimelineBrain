using System;
using Unity.Entities;

namespace Movements.Movement.Data.Tags.Modifiers
{
    [Serializable]
    public struct
        ReverseDirectionEnableableComponent : IComponentData,
        IEnableableComponent // for Boomerang and ping-pong, -1.0 for reverse while calculating
    {
    }
}