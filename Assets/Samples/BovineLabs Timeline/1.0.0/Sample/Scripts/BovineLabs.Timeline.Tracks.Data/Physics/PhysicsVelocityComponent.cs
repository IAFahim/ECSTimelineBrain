using Unity.Entities;
using Unity.Physics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PhysicsVelocityComponent : IComponentData
    {
        public PhysicsVelocity PhysicsVelocity;
        public bool IsLocalSpace;
    }
}