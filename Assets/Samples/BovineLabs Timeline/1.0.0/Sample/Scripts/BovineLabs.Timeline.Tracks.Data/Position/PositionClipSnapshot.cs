namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;
    using Unity.Mathematics;

    // Stores the position of the bound entity at the moment the clip starts.
    public struct PositionClipSnapshot : IComponentData
    {
        public float3 Value;
        public bool IsCaptured;
    }
}