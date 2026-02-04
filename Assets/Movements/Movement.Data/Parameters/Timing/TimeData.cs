using Unity.Entities;

namespace Movements.Movement.Data.Parameters.Timing
{
    /// <summary>
    ///     Time data wrapper for singleton injection into Facets.
    ///     Used with [Singleton] attribute in IFacet definitions.
    /// </summary>
    public struct TimeData : IComponentData
    {
        public float DeltaTime;
    }
}