using Unity.Entities;

namespace Movements.Movement.Data.Tags.Modifiers
{
    /// <summary>
    /// When enabled, movement reverses direction when reaching the end position.
    /// Uses IEnableableComponent to toggle boomerang behavior at runtime.
    /// </summary>
    public struct BoomerangTag : IComponentData, IEnableableComponent
    {
    }
}
