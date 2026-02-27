using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public float SearchRadius = 5f;

    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new SpatialMapsSearch { SearchRadius = authoring.SearchRadius });
            AddBuffer<SpatialMapsNeighbours>(entity);
        }
    }
}