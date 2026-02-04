using Unity.Entities;

namespace Movements.Movement.Data.Tags.Modifiers
{
    /// <summary>
    /// When enabled, movement oscillates back and forth between start and end positions.
    /// Uses IEnableableComponent to toggle ping-pong behavior at runtime.
    /// </summary>
    public struct PingPongTag : IComponentData, IEnableableComponent
    {
    }
}
