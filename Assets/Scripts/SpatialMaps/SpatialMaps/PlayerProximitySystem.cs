using BovineLabs.Core.Spatial;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerProximitySystem : ISystem
{
    private EntityQuery playerQuery;
    private PositionBuilder positionBuilder;
    private SpatialMap<SpatialPosition> spatialMap;

    public void OnCreate(ref SystemState state)
    {
        // Query for all players that have a transform
        playerQuery = SystemAPI.QueryBuilder()
            .WithAllRW<Player>()
            .WithAll<LocalTransform>()
            .Build();

        // PositionBuilder is a highly-optimized helper to grab positions out of a query
        positionBuilder = new PositionBuilder(ref state, playerQuery);

        // Initialize SpatialMap:
        // parameter 1: Cell Size (e.g., 2.0f means 2x2 unit grids)
        // parameter 2: Total World Size (e.g., 1000 units). Needed for hashing math.
        spatialMap = new SpatialMap<SpatialPosition>(2.0f, 1000, Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        spatialMap.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (playerQuery.IsEmpty)
            return;

        // 1. Gather all player positions. 
        // The resulting array maps 1:1 with the [EntityIndexInQuery] used in IJobEntity later!
        state.Dependency = positionBuilder.Gather(ref state, state.Dependency, out var positions);

        // 2. Build the spatial map. This efficiently hashes all points into buckets.
        state.Dependency = spatialMap.Build(positions, state.Dependency);

        // 3. Find neighbors
        state.Dependency = new ProximityJob
        {
            Positions = positions,
            SpatialMap = spatialMap.AsReadOnly(), // Pass read-only map to the job
            SearchRadius = 2.0f,
            SearchRadiusSq = 4.0f // Pre-squared for math.distancesq
        }.ScheduleParallel(playerQuery, state.Dependency);
        
        // Note: We don't dispose 'positions' because PositionBuilder allocates it using 
        // WorldRewindableAllocator, which automatically cleans up every frame!
    }

    [BurstCompile]
    public partial struct ProximityJob : IJobEntity
    {
        [ReadOnly] public NativeArray<SpatialPosition> Positions;
        [ReadOnly] public SpatialMap.ReadOnly SpatialMap;

        public float SearchRadius;
        public float SearchRadiusSq;

        // [EntityIndexInQuery] gives us the exact index into the Positions array
        private void Execute([EntityIndexInQuery] int index, ref Player player)
        {
            float3 myPos = Positions[index].Position;

            // Get the X/Z bounds for our search radius to know which cells to check
            int2 minCell = SpatialMap.Quantized(myPos.xz - new float2(SearchRadius));
            int2 maxCell = SpatialMap.Quantized(myPos.xz + new float2(SearchRadius));

            player.IsNearAnotherPlayer = false;

            // Iterate over the surrounding grid cells
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    int cellHash = SpatialMap.Hash(new int2(x, y));

                    // If the cell has entities in it...
                    if (SpatialMap.Map.TryGetFirstValue(cellHash, out int otherIndex, out var iterator))
                    {
                        do
                        {
                            // Don't compare against ourselves
                            if (otherIndex == index) 
                                continue;

                            float3 otherPos = Positions[otherIndex].Position;

                            // Actual exact distance check
                            if (math.distancesq(myPos, otherPos) <= SearchRadiusSq)
                            {
                                player.IsNearAnotherPlayer = true;
                                return; // Early exit, we only needed to know if AT LEAST ONE is near
                            }

                        } while (SpatialMap.Map.TryGetNextValue(out otherIndex, ref iterator));
                    }
                }
            }
        }
    }
}