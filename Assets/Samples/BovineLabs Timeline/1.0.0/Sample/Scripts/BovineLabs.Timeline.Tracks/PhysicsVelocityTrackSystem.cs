// <copyright file="PhysicsVelocityTrackSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using UnityEngine;

namespace BovineLabs.Timeline.Tracks
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Core.Jobs;
    using BovineLabs.Timeline;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Physics;

    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PhysicsVelocityTrackSystem : ISystem
    {
        private TrackBlendImpl<PhysicsVelocity, PhysicsVelocityAnimated> impl;

        public const int INNERLOOP_BATCH_COUNT = 64;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.impl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.impl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(false);
            var blendData = this.impl.Update(ref state);

            state.Dependency = new WriteVelocityJob
            {
                BlendData = blendData,
                VelocityLookup = velocityLookup
            }.ScheduleParallel(blendData, INNERLOOP_BATCH_COUNT, state.Dependency);
        }

        [BurstCompile]
        private partial struct WriteVelocityJob : IJobParallelHashMapDefer
        {
            [ReadOnly]
            public NativeParallelHashMap<Entity, MixData<PhysicsVelocity>>.ReadOnly BlendData;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<PhysicsVelocity> VelocityLookup;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(this.BlendData, entryIndex, out var entity, out var mixResult);

                var velocity = this.VelocityLookup.GetRefRWOptional(entity);
                if (!velocity.IsValid)
                {
                    return;
                }

                var timelineContribution = JobHelpers.Blend<PhysicsVelocity, PhysicsVelocityMixer>(ref mixResult, default);

                velocity.ValueRW.Linear += timelineContribution.Linear;
                velocity.ValueRW.Angular += timelineContribution.Angular;
                Debug.Log($"L: {velocity.ValueRO.Linear} A: {velocity.ValueRO.Angular}");
            }
        }
    }
}
