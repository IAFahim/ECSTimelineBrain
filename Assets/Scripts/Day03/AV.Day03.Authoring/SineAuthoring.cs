using AV.Day03.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Day03.Authoring
{
    public class SineAuthoring : MonoBehaviour
    {
        public GameObject prefabToSpawn;
        public float speed = 2.0f;
        public float amplitude = 2.0f;

        class Baker : Baker<SineAuthoring>
        {
            public override void Bake(SineAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new SineMovement
                {
                    speed = authoring.speed,
                    amplitude = authoring.amplitude,
                    originalPosition = float3.zero,
                    elapsedTime = 0,
                });
                SetComponentEnabled<SineMovement>(entity,false);

                AddComponent(entity, new SineSpawner
                {
                    Prefab = authoring.prefabToSpawn
                });
            }
        }
    }
}
