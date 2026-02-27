using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data.Parenting;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace BovineLabs.Timeline.Tracks
{[UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct UnParentingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UnParentComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var parallelEcb = ecb.AsParallelWriter();

            var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
            var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var parentLookup = SystemAPI.GetComponentLookup<Parent>(true);

            var unparentJob = new UnparentJob
            {
                ECB = parallelEcb,
                LocalToWorldLookup = localToWorldLookup,
                LocalTransformLookup = localTransformLookup,
                ParentLookup = parentLookup
            };
            state.Dependency = unparentJob.ScheduleParallel(state.Dependency);

            var reparentJob = new ReparentJob
            {
                ECB = parallelEcb
            };
            state.Dependency = reparentJob.ScheduleParallel(state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile][WithAll(typeof(ClipActive))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct UnparentJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly] public ComponentLookup<Parent> ParentLookup;

            private void Execute([ChunkIndexInQuery] int chunkIndex, ref UnParentComponent unparent, in TrackBinding binding)
            {
                var target = binding.Value;

                if (!ParentLookup.HasComponent(target)) return;

                if (LocalTransformLookup.TryGetComponent(target, out var originalLT))
                {
                    unparent.OriginalLocalTransform = originalLT;
                }

                if (LocalToWorldLookup.TryGetComponent(target, out var targetLTW))
                {
                    var newLT = ExtractLocalTransform(targetLTW.Value);
                    ECB.SetComponent(chunkIndex, target, newLT);
                }

                ECB.RemoveComponent<Parent>(chunkIndex, target);
                ECB.RemoveComponent<PreviousParent>(chunkIndex, target);
            }
        }

        [BurstCompile]
        [WithNone(typeof(ClipActive))][WithAll(typeof(ClipActivePrevious))]
        private partial struct ReparentJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            private void Execute([ChunkIndexInQuery] int chunkIndex, in UnParentComponent unparent, in TrackBinding binding)
            {
                var target = binding.Value;
                var parent = unparent.LastParent;

                if (parent == Entity.Null) return;

                ECB.SetComponent(chunkIndex, target, unparent.OriginalLocalTransform);

                ECB.AddComponent(chunkIndex, target, new Parent { Value = parent });
            }
        }

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