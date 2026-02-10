using Unity.Entities;
using Unity.Physics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PhysicsVelocityTargetComponent : IComponentData
    {
        public PhysicsVelocity value;
    }
}