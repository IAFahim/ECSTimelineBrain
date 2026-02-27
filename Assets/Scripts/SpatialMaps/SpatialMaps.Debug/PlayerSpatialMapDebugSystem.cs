using BovineLabs.Quill;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(PlayerProximitySystem))][WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.Editor)]
public partial struct PlayerNeighboursDebugSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpatialMapsSearch>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var drawer = SystemAPI.GetSingleton<DrawSystem.Singleton>().CreateDrawer();
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        
        state.Dependency = new DrawNeighboursJob
        {
            Drawer = drawer,
            TransformLookup = transformLookup
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    private partial struct DrawNeighboursJob : IJobEntity
    {
        public Drawer Drawer;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

        private void Execute(in LocalTransform transform, in SpatialMapsSearch spatialMapsSearch, in DynamicBuffer<SpatialMapsNeighbours> neighbours)
        {
            float3 myPos = transform.Position;

            float3 ringDirection = new float3(0, spatialMapsSearch.SearchRadius, 0);
            Color ringColor = new Color(1f, 1f, 1f, 0.1f);

            Drawer.Circle(myPos, ringDirection, ringColor);

            if (neighbours.IsEmpty) return;

            Color tetherColor = new Color(0.2f, 1f, 0.8f, 0.4f);
            Color anchorColor = new Color(0.2f, 1f, 0.8f, 1.0f);

            float3 offset = new float3(0, 0.1f, 0);
            float3 startPos = myPos + offset;

            foreach (var neighbour in neighbours)
            {
                if (TransformLookup.TryGetComponent(neighbour.Entity, out var targetTransform))
                {
                    float3 targetPos = targetTransform.Position + offset;

                    Drawer.Line(startPos, targetPos, tetherColor);

                    Drawer.Point(targetPos, 0.15f, anchorColor);
                }
            }
        }
    }
}