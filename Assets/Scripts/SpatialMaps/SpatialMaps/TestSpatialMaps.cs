using BovineLabs.Core.Spatial;
using SpatialMaps.SpatialMaps.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpatialMaps.SpatialMaps
{
    public partial struct TestSpatialMaps : ISystem
    {
        private PositionBuilder positionBuilder;
        private SpatialMap<SpatialPosition> spatialMap;
        private EntityQuery query;

        public void OnCreate(ref SystemState state)
        {
            query = SystemAPI.QueryBuilder().WithAll<LocalTransform, Neighbours>().Build();
            this.positionBuilder = new PositionBuilder(ref state, query);

            const int size = 4096;
            const int quantizeStep = 16;

            this.spatialMap = new SpatialMap<SpatialPosition>(quantizeStep, size);
        }

        public void OnDestroy(ref SystemState state)
        {
            this.spatialMap.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency =
                this.positionBuilder.Gather(ref state, state.Dependency, out NativeArray<SpatialPosition> positions);
            state.Dependency = this.spatialMap.Build(positions, state.Dependency);
            var entities = this.query.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var dependency);
            state.Dependency = dependency;
            
            new TestJob
                {
                    Entities = entities.AsDeferredJobArray(),
                    Positions = positions,
                    SpatialMap = this.spatialMap.AsReadOnly(),
                }
                .ScheduleParallel();

        }
        
        [BurstCompile]
        private partial struct TestJob : IJobEntity
        {
            private const float Radius = 10;

            [ReadOnly]
            public NativeArray<Entity> Entities;

            [ReadOnly]
            public NativeArray<SpatialPosition> Positions;

            [ReadOnly]
            public SpatialMap.ReadOnly SpatialMap;

            private void Execute(Entity entity, in LocalTransform localTransform, DynamicBuffer<Neighbours> neighbours)
            {
                neighbours.Clear();

                // Find the min and max boxes
                var min = this.SpatialMap.Quantized(localTransform.Position.xz - Radius);
                var max = this.SpatialMap.Quantized(localTransform.Position.xz + Radius);

                for (var j = min.y; j <= max.y; j++)
                {
                    for (var i = min.x; i <= max.x; i++)
                    {
                        var hash = this.SpatialMap.Hash(new int2(i, j));

                        if (!this.SpatialMap.Map.TryGetFirstValue(hash, out int item, out var it))
                        {
                            continue;
                        }

                        do
                        {
                            var otherEntity = this.Entities[item];

                            // Don't add ourselves
                            if (otherEntity.Equals(entity))
                            {
                                continue;
                            }

                            var otherPosition = this.Positions[item].Position;

                            // The spatialmap serves as the broad-phase but most of the time we still need to ensure entities are actually within range
                            if (math.distancesq(localTransform.Position.xz, otherPosition.xz) <= Radius * Radius)
                            {
                                neighbours.Add(new Neighbours { Entity = otherEntity });
                            }
                        }
                        while (this.SpatialMap.Map.TryGetNextValue(out item, ref it));
                    }

                }
            }
        }

    }
}
