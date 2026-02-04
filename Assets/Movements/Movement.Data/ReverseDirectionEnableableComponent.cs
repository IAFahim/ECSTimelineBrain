using System;
using Unity.Entities;

namespace Movements.Movement.Data
{
    [Serializable]
    public struct ReverseDirectionEnableableComponent : IComponentData, IEnableableComponent // for Boomerang and ping-pong
    {
    }
}