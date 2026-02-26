using Unity.Entities;
using UnityEngine;

namespace SpatialMaps.SpatialMaps.Data
{
    public class NeighboursAuthoring : MonoBehaviour
    {
        public class NeighboursBaker : Baker<NeighboursAuthoring>
        {
            public override void Bake(NeighboursAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddBuffer<Neighbours>(entity);
            }
        }
    }
}