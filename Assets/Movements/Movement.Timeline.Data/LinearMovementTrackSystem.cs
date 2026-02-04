using BovineLabs.Core.Collections;
using BovineLabs.Core.Jobs;
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Movements.Movement.Timeline.Data
{
    /// <summary>
    /// System that processes Linear Movement timeline clips.
    /// Uses TrackBlendImpl to blend between overlapping clips.
    /// </summary>
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [BurstCompile]
    public partial struct LinearMovementTrackSystem : ISystem
    {
        // Blend implementations for position
        private TrackBlendImpl<float3, LinearMovementAnimated> positionImpl;
        private TrackBlendImpl<quaternion, LinearMovementRotationAnimated> rotationImpl;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.positionImpl.OnCreate(ref state);
            this.rotationImpl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.positionImpl.OnDestroy(ref state);
            this.rotationImpl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var localTransforms = SystemAPI.GetComponentLookup<LocalTransform>();

            // Phase 1: Capture start position when timeline activates
            new CaptureStartJob { LocalTransforms = localTransforms }.ScheduleParallel();

            // Phase 2: Reset position when timeline deactivates
            new ResetJob { LocalTransforms = localTransforms }.Schedule();

            // Phase 3: Get blend data from Timeline and write to transforms
            var positionBlend = this.positionImpl.Update(ref state);
            state.Dependency = new WritePositionJob
            {
                BlendData = positionBlend,
                LocalTransforms = localTransforms
            }.ScheduleParallel(positionBlend, 64, state.Dependency);

            var rotationBlend = this.rotationImpl.Update(ref state);
            state.Dependency = new WriteRotationJob
            {
                BlendData = rotationBlend,
                LocalTransforms = localTransforms
            }.ScheduleParallel(rotationBlend, 64, state.Dependency);
        }

        /// <summary>
        /// Job that captures the start position when timeline activates.
        /// Stores it for later use in movement calculation.
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))]
        private partial struct CaptureStartJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(
                ref LinearMovementStartPosition startPos,
                in TrackBinding trackBinding)
            {
                if (!this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    startPos.IsValid = false;
                    return;
                }

                startPos.Position = bindingTransform.Position;
                startPos.Rotation = bindingTransform.Rotation;
                startPos.IsValid = true;
            }
        }

        /// <summary>
        /// Job that resets to start position when timeline deactivates.
        /// </summary>
        [BurstCompile]
        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious))]
        private partial struct ResetJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(
                in LinearMovementStartPosition startPos,
                in LinearMovementResetOnDeactivate reset,
                in TrackBinding trackBinding)
            {
                if (!startPos.IsValid)
                {
                    return;
                }

                var lt = this.LocalTransforms.GetRefRWOptional(trackBinding.Value);
                if (!lt.IsValid)
                {
                    return;
                }

                if (reset.ResetPosition)
                {
                    lt.ValueRW.Position = startPos.Position;
                }

                if (reset.ResetRotation)
                {
                    lt.ValueRW.Rotation = startPos.Rotation;
                }
            }
        }

        /// <summary>
        /// Job that writes blended position values to LocalTransform.
        /// Uses IJobParallelHashMapDefer for efficient iteration over blend data.
        /// </summary>
        [BurstCompile]
        private struct WritePositionJob : IJobParallelHashMapDefer
        {
            [ReadOnly]
            public NativeParallelHashMap<Entity, MixData<float3>>.ReadOnly BlendData;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransforms;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                // Read the blend data for this entry
                this.Read(this.BlendData, entryIndex, out var entity, out var target);

                var lt = this.LocalTransforms.GetRefRWOptional(entity);
                if (!lt.IsValid)
                {
                    return;
                }

                // Write blended position to LocalTransform
                lt.ValueRW.Position = JobHelpers.Blend<float3, Float3Mixer>(ref target, lt.ValueRO.Position);
            }
        }

        /// <summary>
        /// Job that writes blended rotation values to LocalTransform.
        /// </summary>
        [BurstCompile]
        private struct WriteRotationJob : IJobParallelHashMapDefer
        {
            [ReadOnly]
            public NativeParallelHashMap<Entity, MixData<quaternion>>.ReadOnly BlendData;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransforms;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(this.BlendData, entryIndex, out var entity, out var target);

                var lt = this.LocalTransforms.GetRefRWOptional(entity);
                if (!lt.IsValid)
                {
                    return;
                }

                // Write blended rotation to LocalTransform
                lt.ValueRW.Rotation = JobHelpers.Blend<quaternion, QuaternionMixer>(ref target, lt.ValueRO.Rotation);
            }
        }
    }
}
