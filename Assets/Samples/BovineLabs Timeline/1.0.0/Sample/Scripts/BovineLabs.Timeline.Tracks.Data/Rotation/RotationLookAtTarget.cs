using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct RotationLookAtTarget : IComponentData
    {
        public Entity Target;
    }
}