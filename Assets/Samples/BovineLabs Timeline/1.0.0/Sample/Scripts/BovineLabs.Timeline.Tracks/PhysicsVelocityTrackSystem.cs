using BovineLabs.Core.Jobs;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PhysicsVelocityTrackSystem : ISystem
    {
        private TrackBlendImpl<PhysicsVelocity, PhysicsVelocityAnimated> impl;

        public const int INNERLOOP_BATCH_COUNT = 64;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            impl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            impl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
            var blendData = impl.Update(ref state);

            state.Dependency = new WriteVelocityJob
            {
                BlendData = blendData,
                VelocityLookup = velocityLookup
            }.ScheduleParallel(blendData, INNERLOOP_BATCH_COUNT, state.Dependency);
        }

        [BurstCompile]
        private struct WriteVelocityJob : IJobParallelHashMapDefer
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<PhysicsVelocity>>.ReadOnly BlendData;

            [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsVelocity> VelocityLookup;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(BlendData, entryIndex, out var entity, out var mixResult);

                var velocityRW = VelocityLookup.GetRefRW(entity);

                var weights = mixResult.Weights;
                var linearAccum = float3.zero;
                var angularAccum = float3.zero;

                if (weights.x > math.EPSILON)
                {
                    linearAccum += mixResult.Value1.Linear * weights.x;
                    angularAccum += mixResult.Value1.Angular * weights.x;
                }

                if (weights.y > math.EPSILON)
                {
                    linearAccum += mixResult.Value2.Linear * weights.y;
                    angularAccum += mixResult.Value2.Angular * weights.y;
                }

                if (weights.z > math.EPSILON)
                {
                    linearAccum += mixResult.Value3.Linear * weights.z;
                    angularAccum += mixResult.Value3.Angular * weights.z;
                }

                if (weights.w > math.EPSILON)
                {
                    linearAccum += mixResult.Value4.Linear * weights.w;
                    angularAccum += mixResult.Value4.Angular * weights.w;
                }

                velocityRW.ValueRW.Linear += linearAccum;
                velocityRW.ValueRW.Angular += angularAccum;
            }
        }
    }
}