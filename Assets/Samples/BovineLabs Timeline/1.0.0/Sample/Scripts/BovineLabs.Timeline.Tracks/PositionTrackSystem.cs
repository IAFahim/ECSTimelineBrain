using BovineLabs.Core.Jobs;

namespace BovineLabs.Timeline.Tracks
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Timeline;
    using BovineLabs.Timeline.Data;
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PositionTrackSystem : ISystem
    {
        private TrackBlendImpl<float3, PositionAnimated> impl;

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
            var localTransforms = SystemAPI.GetComponentLookup<LocalTransform>(false); // ReadWrite for Reset/Write
            var readOnlyTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true); // ReadOnly for Snapshotting

            // 1. Handle Track Reset (Restore position when timeline stops)
            new ActivateResetJob { LocalTransforms = readOnlyTransforms }.ScheduleParallel();
            new DeactivateResetJob { LocalTransforms = localTransforms }.Schedule();

            // 2. Snapshot positions when clips start
            // This fixes the "flying away" bug by grabbing the base position only once per clip activation
            new CaptureSnapshotJob
            {
                LocalTransforms = readOnlyTransforms
            }.ScheduleParallel();

            // 3. Update 'PositionAnimated' values based on logic
            new CalculatePositionOffsetJob().ScheduleParallel();
            new CalculatePositionTargetJob { LocalTransforms = readOnlyTransforms }.ScheduleParallel();
            new CalculateMoveToStartJob().ScheduleParallel();

            // 4. Blend all the clips together
            var blendData = this.impl.Update(ref state);

            // 5. Write final result to the entity
            state.Dependency = new WritePositionJob 
            { 
                BlendData = blendData, 
                LocalTransforms = localTransforms 
            }.ScheduleParallel(blendData, 64, state.Dependency);
        }

        // --- JOBS ---

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))]
        private partial struct ActivateResetJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionResetOnDeactivate positionResetOnDeactivate, in TrackBinding trackBinding)
            {
                if (this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    positionResetOnDeactivate.Value = bindingTransform.Position;
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious))]
        private partial struct DeactivateResetJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(in PositionResetOnDeactivate positionResetOnDeactivate, in TrackBinding trackBinding)
            {
                var localTransform = this.LocalTransforms.GetRefRWOptional(trackBinding.Value);
                if (localTransform.IsValid)
                {
                    localTransform.ValueRW.Position = positionResetOnDeactivate.Value;
                }
            }
        }

        // Runs when a clip becomes active. Captures the current position of the bound object.
        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithNone(typeof(ClipActivePrevious))] 
        private partial struct CaptureSnapshotJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionClipSnapshot snapshot, in TrackBinding trackBinding)
            {
                if (this.LocalTransforms.TryGetComponent(trackBinding.Value, out var bindingTransform))
                {
                    snapshot.Value = bindingTransform.Position;
                    snapshot.IsCaptured = true;
                }
            }
        }

        // Updates the Animated value based on the Snapshot + Offset
        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct CalculatePositionOffsetJob : IJobEntity
        {
            private void Execute(ref PositionAnimated positionAnimated, in PositionOffset positionOffset, in PositionClipSnapshot snapshot)
            {
                if (!snapshot.IsCaptured) return;

                // Simple additive offset. 
                // Note: If you want Local offset (relative to rotation), you'd need to snapshot rotation too.
                // Assuming World/Simple translation for this sample.
                positionAnimated.Value = snapshot.Value + positionOffset.Offset;
            }
        }

        // Updates the Animated value based on an external Target Entity
        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct CalculatePositionTargetJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransforms;

            private void Execute(ref PositionAnimated positionAnimated, in PositionTarget positionTarget)
            {
                if (!this.LocalTransforms.TryGetComponent(positionTarget.Target, out var targetTransform))
                {
                    return;
                }

                if (positionTarget.Type == OffsetType.Local)
                {
                    positionAnimated.Value = targetTransform.TransformPoint(positionTarget.Offset);
                }
                else
                {
                    positionAnimated.Value = targetTransform.Position + positionTarget.Offset;
                }
            }
        }

        // For "Start Clip", just hold the value captured in the snapshot
        [BurstCompile]
        [WithAll(typeof(ClipActive), typeof(PositionMoveToStart))]
        private partial struct CalculateMoveToStartJob : IJobEntity
        {
            private void Execute(ref PositionAnimated positionAnimated, in PositionClipSnapshot snapshot)
            {
                if (snapshot.IsCaptured)
                {
                    positionAnimated.Value = snapshot.Value;
                }
            }
        }

        [BurstCompile]
        private struct WritePositionJob : IJobParallelHashMapDefer
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<float3>>.ReadOnly BlendData;
            [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> LocalTransforms;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(this.BlendData, entryIndex, out var entity, out var mixData);

                var lt = this.LocalTransforms.GetRefRWOptional(entity);
                if (!lt.IsValid)
                {
                    return;
                }

                // If weights sum < 1 (e.g. gap in timeline), blend towards current position (MixData default)
                // or keep current position.
                lt.ValueRW.Position = JobHelpers.Blend<float3, Float3Mixer>(ref mixData, lt.ValueRO.Position);
            }
        }
    }
}