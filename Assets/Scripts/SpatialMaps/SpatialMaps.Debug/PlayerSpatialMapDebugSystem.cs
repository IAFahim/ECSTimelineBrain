using BovineLabs.Quill;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(PlayerProximitySystem))][WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ServerSimulation)]
public partial struct PlayerNeighboursDebugSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Require Quill's DrawSystem
        state.RequireForUpdate<DrawSystem.Singleton>();
        state.RequireForUpdate<SpatialMapsSearch>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Create the drawer and automatically link it to the Editor Toolbar
        var drawer = SystemAPI.GetSingleton<DrawSystem.Singleton>().CreateDrawer();

        // 2. We need a lookup to get the actual world positions of the neighbour Entities
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        // 3. Schedule the drawing across all worker threads
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
            
            // 1. Draw a faint radar ring showing this spatialMapsSearch's exact SearchRadius
            // In Quill, passing (0, Radius, 0) to Direction draws a flat XZ circle of that radius.
            float3 ringDirection = new float3(0, spatialMapsSearch.SearchRadius, 0);
            Color ringColor = new Color(1f, 1f, 1f, 0.1f); // Faint transparent white
            
            Drawer.Circle(myPos, ringDirection, ringColor);

            if (neighbours.IsEmpty) return;

            // 2. Setup colors for the tethers
            Color tetherColor = new Color(0.2f, 1f, 0.8f, 0.4f); // Semi-transparent Neon Cyan/Mint
            Color anchorColor = new Color(0.2f, 1f, 0.8f, 1.0f); // Solid Neon Cyan/Mint

            // Elevate lines slightly so they don't Z-fight with the ground plane
            float3 offset = new float3(0, 0.1f, 0);
            float3 startPos = myPos + offset;

            // 3. Draw sleek connection lines to all confirmed neighbors
            foreach (var neighbour in neighbours)
            {
                // Safely lookup the neighbor's current transform
                if (TransformLookup.TryGetComponent(neighbour.Entity, out var targetTransform))
                {
                    float3 targetPos = targetTransform.Position + offset;

                    // Draw the tether line connecting the two players
                    Drawer.Line(startPos, targetPos, tetherColor);
                    
                    // Draw a crisp little solid point at the target's base to anchor the visual
                    Drawer.Point(targetPos, 0.15f, anchorColor);
                }
            }
        }
    }
}