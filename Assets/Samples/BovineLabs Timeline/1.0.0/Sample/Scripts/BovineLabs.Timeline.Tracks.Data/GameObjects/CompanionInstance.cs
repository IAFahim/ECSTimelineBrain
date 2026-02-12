using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data
{
    // We use ICleanupComponentData to ensure we can destroy the 
    // spawned instance even if the Timeline/Clip entity itself is destroyed abruptly.
    public struct CompanionInstance : ICleanupComponentData
    {
        public Entity Value;
    }
}