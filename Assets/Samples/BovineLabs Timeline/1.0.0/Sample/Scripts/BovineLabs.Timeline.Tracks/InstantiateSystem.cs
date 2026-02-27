using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Instantiates;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct InstantiateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InstantiateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var parallelEcb = ecb.AsParallelWriter();

            var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);

            var instantiateJob = new InstantiateJob
            {
                ECB = parallelEcb,
                LocalToWorldLookup = localToWorldLookup
            };

            state.Dependency = instantiateJob.ScheduleParallel(state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct InstantiateJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;

            private void Execute([ChunkIndexInQuery] int chunkIndex, in TrackBinding binding,
                in InstantiateComponent instantiate)
            {
                var target = binding.Value;
                var instance = ECB.Instantiate(chunkIndex, instantiate.Prefab);
                if (instantiate.Parent)
                {
                    ECB.AddComponent(chunkIndex, instance, new Parent { Value = target });
                }
                else
                {
                    if (!LocalToWorldLookup.TryGetComponent(target, out var targetLtw)) return;
                    var spawnTransform = ExtractLocalTransform(targetLtw.Value);
                    ECB.SetComponent(chunkIndex, instance, spawnTransform);
                }
            }
        }

        /// <summary>
        ///     Safely converts a float4x4 into a LocalTransform structure.
        /// </summary>
        private static LocalTransform ExtractLocalTransform(float4x4 m)
        {
            var pos = m.c3.xyz;
            var scale3 = new float3(math.length(m.c0.xyz), math.length(m.c1.xyz), math.length(m.c2.xyz));
            var scale = scale3.x;

            if (scale > 1e-6f)
            {
                m.c0.xyz /= scale3.x;
                m.c1.xyz /= scale3.y;
                m.c2.xyz /= scale3.z;
            }

            return LocalTransform.FromPositionRotationScale(pos, new quaternion(m), scale);
        }
    }
}