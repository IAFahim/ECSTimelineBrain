using Unity.Entities;
using Unity.Transforms;

namespace Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data.Parenting
{
    public struct UnParentComponent : IComponentData
    {
        public Entity LastParent;
        public LocalTransform OriginalLocalTransform;
    }
}