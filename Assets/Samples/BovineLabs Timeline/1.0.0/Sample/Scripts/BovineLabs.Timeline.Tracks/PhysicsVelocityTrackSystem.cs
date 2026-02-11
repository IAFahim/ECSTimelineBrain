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

            // TODO all these could be run in parallel
            new ActivateResetJob { VelocityLookup = velocityLookup }.ScheduleParallel();
            new DeactivateResetJob { VelocityLookup = velocityLookup }.Schedule();
            new PhysicsVelocityOffsetJob { VelocityLookup = velocityLookup }.ScheduleParallel();
            new PhysicsVelocityTargetJob { VelocityLookup = velocityLookup }.ScheduleParallel();
            new MoveToStartingVelocityClipJob { VelocityLookup = velocityLookup }.ScheduleParallel();

            var blendData = this.impl.Update(ref state);

            state.Dependency = new WriteVelocityJob
            {
                BlendData = blendData,
                VelocityLookup = velocityLookup
            }.ScheduleParallel(blendData, INNERLOOP_BATCH_COUNT, state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))]
        private partial struct ActivateResetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<PhysicsVelocity> VelocityLookup;

            private void Execute(ref PhysicsVelocityResetOnDeactivate velocityResetOnDeactivate, in TrackBinding trackBinding)
            {
                if (!this.VelocityLookup.TryGetComponent(trackBinding.Value, out var bindingVelocity))
                {
                    return;
                }

                velocityResetOnDeactivate.Value = bindingVelocity;
            }
        }

        [BurstCompile]
        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious))]
        private partial struct DeactivateResetJob : IJobEntity
        {
            public ComponentLookup<PhysicsVelocity> VelocityLookup;

            private void Execute(in PhysicsVelocityResetOnDeactivate velocityResetOnDeactivate, in TrackBinding trackBinding)
            {
                var has = this.VelocityLookup.TryGetRefRW(trackBinding.Value, out var physicsVelocity);
                if (!has) return;
                
                physicsVelocity.ValueRW.Linear = velocityResetOnDeactivate.Value.Linear;
                physicsVelocity.ValueRW.Angular = velocityResetOnDeactivate.Value.Angular;
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))] // we only update this once and cache it
        private partial struct PhysicsVelocityOffsetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<PhysicsVelocity> VelocityLookup;

            private void Execute(ref PhysicsVelocityAnimated physicsVelocityAnimated, in PhysicsVelocityOffset physicsVelocityOffset, in TrackBinding trackBinding)
            {
                if (!this.VelocityLookup.TryGetComponent(trackBinding.Value, out var bindingVelocity))
                {
                    return;
                }

                physicsVelocityAnimated.Value = new PhysicsVelocity
                {
                    Linear = bindingVelocity.Linear + physicsVelocityOffset.Value.Linear,
                    Angular = bindingVelocity.Angular + physicsVelocityOffset.Value.Angular
                };
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        private partial struct PhysicsVelocityTargetJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<PhysicsVelocity> VelocityLookup;

            private void Execute(ref PhysicsVelocityAnimated physicsVelocityAnimated, in PhysicsVelocityTarget physicsVelocityTarget)
            {
                if (!this.VelocityLookup.TryGetComponent(physicsVelocityTarget.Target, out var targetVelocity))
                {
                    return;
                }

                physicsVelocityAnimated.Value = new PhysicsVelocity
                {
                    Linear = targetVelocity.Linear + physicsVelocityTarget.LinearOffset,
                    Angular = targetVelocity.Angular + physicsVelocityTarget.AngularOffset
                };
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))] // we only update this once and cache it
        [WithAll(typeof(PhysicsVelocityMoveToStart))]
        private partial struct MoveToStartingVelocityClipJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<PhysicsVelocity> VelocityLookup;

            private void Execute(ref PhysicsVelocityAnimated physicsVelocityAnimated, in TrackBinding trackBinding)
            {
                if (!this.VelocityLookup.TryGetComponent(trackBinding.Value, out var bindingVelocity))
                {
                    return;
                }

                physicsVelocityAnimated.Value = bindingVelocity;
            }
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
