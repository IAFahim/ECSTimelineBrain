using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct CompanionInstance : ICleanupComponentData
    {
        public Entity Value;
    }
}