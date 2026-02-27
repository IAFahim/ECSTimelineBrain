using BovineLabs.Core.Spatial;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerProximitySystem : ISystem
{
    private EntityQuery playerQuery;
    private PositionBuilder positionBuilder;
    private SpatialMap<SpatialPosition> spatialMap;

    // Exposed for the Debug Gizmo / Quill Drawer
    public float CellSize => 2.0f;
    public int WorldSize => 1000;
    public SpatialMap<SpatialPosition> Map => spatialMap;

    public void OnCreate(ref SystemState state)
    {
        playerQuery = SystemAPI.QueryBuilder()
            .WithAll<SpatialMapsSearch, LocalTransform>()
            .WithAllRW<SpatialMapsNeighbours>() // We are writing to this!
            .Build();

        positionBuilder = new PositionBuilder(ref state, playerQuery);
        spatialMap = new SpatialMap<SpatialPosition>(CellSize, WorldSize, Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        spatialMap.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (playerQuery.IsEmpty) return;

        // 1. Gather all positions
        state.Dependency = positionBuilder.Gather(ref state, state.Dependency, out var positions);

        // 2. Build Spatial Map
        state.Dependency = spatialMap.Build(positions, state.Dependency);

        // 3. To store Entities in the buffer, we need an index-to-Entity mapping.
        // ToEntityListAsync uses WorldUpdateAllocator, meaning it allocates 0 garbage and automatically frees at the end of the frame!
        var entities = playerQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var entityDeps);
        state.Dependency = JobHandle.CombineDependencies(state.Dependency, entityDeps);

        // 4. Run the Proximity Job
        state.Dependency = new FindNeighboursJob
        {
            Positions = positions,
            Entities = entities.AsDeferredJobArray(), // Pass the matching entity array
            SpatialMap = spatialMap.AsReadOnly()
        }.ScheduleParallel(playerQuery, state.Dependency);
    }

    [BurstCompile]
    public partial struct FindNeighboursJob : IJobEntity
    {
        [ReadOnly] public NativeArray<SpatialPosition> Positions;
        [ReadOnly] public NativeArray<Entity> Entities;
        [ReadOnly] public SpatialMap.ReadOnly SpatialMap;

        private void Execute([EntityIndexInQuery] int index, in SpatialMapsSearch spatialMapsSearch, ref DynamicBuffer<SpatialMapsNeighbours> neighbours)
        {
            // Clear the buffer from the previous frame
            neighbours.Clear();

            float3 myPos = Positions[index].Position;
            float radius = spatialMapsSearch.SearchRadius;
            float radiusSq = radius * radius;

            // Get the X/Z bounds based on this specific spatialMapsSearch's SearchRadius
            int2 minCell = SpatialMap.Quantized(myPos.xz - new float2(radius));
            int2 maxCell = SpatialMap.Quantized(myPos.xz + new float2(radius));

            // Iterate over all cells that intersect this spatialMapsSearch's specific bounding box
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int cellHash = SpatialMap.Hash(new int2(x, y));

                    // If the cell contains targets...
                    if (SpatialMap.Map.TryGetFirstValue(cellHash, out int otherIndex, out var iterator))
                    {
                        do
                        {
                            // Skip checking distance against ourselves
                            if (otherIndex == index) continue;

                            float3 otherPos = Positions[otherIndex].Position;

                            // If within this spatialMapsSearch's custom radius, add them to the buffer!
                            if (math.distancesq(myPos, otherPos) <= radiusSq)
                            {
                                // Map the otherIndex back to its actual Entity using our Entities array!
                                neighbours.Add(new SpatialMapsNeighbours { Entity = Entities[otherIndex] });
                            }

                        } while (SpatialMap.Map.TryGetNextValue(out otherIndex, ref iterator));
                    }
                }
            }
        }
    }
}