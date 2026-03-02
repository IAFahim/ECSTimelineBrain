using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct RotationResetOnDeactivate : IComponentData
    {
        public quaternion Value;
    }
}