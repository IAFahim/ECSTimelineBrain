using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.GameObjects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct ActivationTrackSystem : ISystem
    {
        private TrackBlendImpl<bool, ActivationAnimatedComponent> impl;

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
            var disabledLookup = SystemAPI.GetComponentLookup<Disabled>(true);
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

            new CaptureOriginalStateJob
            {
                DisabledLookup = disabledLookup
            }.ScheduleParallel();

            new ApplyPostPlaybackStateJob
            {
                DisabledLookup = disabledLookup,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
            }.Schedule();

            var blendData = this.impl.Update(ref state);

            state.Dependency = new ApplyRuntimeActivationJob
            {
                BlendData = blendData,
                DisabledLookup = disabledLookup,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))]
        private partial struct CaptureOriginalStateJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;

            private void Execute(ref ActivationTrackComponent trackData, in TrackBinding binding)
            {
                trackData.OriginalWasDisabled = this.DisabledLookup.HasComponent(binding.Value);
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        private partial struct ApplyRuntimeActivationJob : IJobEntity
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<bool>>.ReadOnly BlendData;
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;
            public EntityCommandBuffer.ParallelWriter ECB;

            private void Execute(Entity trackEntity, [EntityIndexInQuery] int sortKey, in TrackBinding binding)
            {
                bool shouldBeActive = this.BlendData.ContainsKey(binding.Value);

                bool isCurrentlyDisabled = this.DisabledLookup.HasComponent(binding.Value);

                if (shouldBeActive && isCurrentlyDisabled)
                {
                    this.ECB.RemoveComponent<Disabled>(sortKey, binding.Value);
                }
                else if (!shouldBeActive && !isCurrentlyDisabled)
                {
                    this.ECB.AddComponent<Disabled>(sortKey, binding.Value);
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious))]
        private partial struct ApplyPostPlaybackStateJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;
            public EntityCommandBuffer ECB;

            private void Execute(in ActivationTrackComponent trackData, in TrackBinding binding)
            {
                bool isCurrentlyDisabled = this.DisabledLookup.HasComponent(binding.Value);

                switch (trackData.PostPlaybackState)
                {
                    case PostPlaybackState.Active:
                        if (isCurrentlyDisabled)
                            this.ECB.RemoveComponent<Disabled>(binding.Value);
                        break;

                    case PostPlaybackState.Inactive:
                        if (!isCurrentlyDisabled)
                            this.ECB.AddComponent<Disabled>(binding.Value);
                        break;

                    case PostPlaybackState.Revert:
                        if (trackData.OriginalWasDisabled && !isCurrentlyDisabled)
                        {
                            this.ECB.AddComponent<Disabled>(binding.Value);
                        }
                        else if (!trackData.OriginalWasDisabled && isCurrentlyDisabled)
                        {
                            this.ECB.RemoveComponent<Disabled>(binding.Value);
                        }

                        break;

                    case PostPlaybackState.LeaveAsIs:
                        break;
                }
            }
        }
    }
}