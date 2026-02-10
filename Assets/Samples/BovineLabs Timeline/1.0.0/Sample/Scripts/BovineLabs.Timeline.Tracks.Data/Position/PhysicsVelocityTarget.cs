using Unity.Entities;
using Unity.Physics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PhysicsVelocityTarget : IComponentData
    {
        public PhysicsVelocity Value;
    }
}