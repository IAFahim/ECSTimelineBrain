using AV.Day03.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Day03.Authoring
{
    public class SineAuthoring : MonoBehaviour
    {
        public GameObject PrefabToSpawn;
        public float Speed = 2.0f;
        public float Amplitude = 2.0f;

        class Baker : Baker<SineAuthoring>
        {
            public override void Bake(SineAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new SineMovement
                {
                    Speed = authoring.Speed,
                    Amplitude = authoring.Amplitude,
                    OriginalPosition = float3.zero
                });

                AddComponent(entity, new SineSpawner
                {
                    Prefab = authoring.PrefabToSpawn
                });
            }
        }
    }
}
