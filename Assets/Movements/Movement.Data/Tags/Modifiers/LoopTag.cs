using Unity.Entities;

namespace Movements.Movement.Data.Tags.Modifiers
{
    /// <summary>
    /// When present, movement loops back to start position when reaching the end.
    /// This is a regular tag component (not enableable) - always active when present.
    /// </summary>
    public struct LoopTag : IComponentData
    {
    }
}
