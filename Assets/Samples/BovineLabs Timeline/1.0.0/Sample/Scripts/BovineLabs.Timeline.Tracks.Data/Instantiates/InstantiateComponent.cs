using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data.Instantiates
{
    public struct InstantiateComponent : IComponentData
    {
        public Entity Prefab;
        public bool Parent;
    }
}