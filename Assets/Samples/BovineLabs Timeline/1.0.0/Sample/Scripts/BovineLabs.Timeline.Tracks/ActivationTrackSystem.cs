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
        private TrackBlendImpl<bool, ActivationAnimatedComponent> _impl;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _impl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _impl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var disabledLookup = SystemAPI.GetComponentLookup<Disabled>(true);
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

            new ApplyPostPlaybackStateJob
            {
                DisabledLookup = disabledLookup,
                OriginalWasDisabledTagLookup =  SystemAPI.GetComponentLookup<OriginalWasDisabledTag>(true),
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
            }.Schedule();

            var blendData = _impl.Update(ref state);

            state.Dependency = new ApplyRuntimeActivationJob
            {
                BlendData = blendData,
                DisabledLookup = disabledLookup,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel(state.Dependency);
        }
        
        
        [BurstCompile]
        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious), typeof(ActivationTrackComponent))]
        private partial struct ApplyPostPlaybackStateJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;
            [ReadOnly] public ComponentLookup<OriginalWasDisabledTag> OriginalWasDisabledTagLookup;
            public EntityCommandBuffer ECB;

            private void Execute(in ActivationTrackComponent trackData, in TrackBinding binding)
            {
                bool isCurrentlyDisabled = DisabledLookup.HasComponent(binding.Value);

                switch (trackData.PostPlaybackState)
                {
                    case ActivationTrack.PostPlaybackState.Active:
                        if (isCurrentlyDisabled) ECB.RemoveComponent<Disabled>(binding.Value);
                        break;

                    case ActivationTrack.PostPlaybackState.Inactive:
                        if (!isCurrentlyDisabled) ECB.AddComponent<Disabled>(binding.Value);
                        break;

                    case ActivationTrack.PostPlaybackState.Revert:
                        switch (OriginalWasDisabledTagLookup.HasComponent(binding.Value))
                        {
                            case true when !isCurrentlyDisabled:
                                ECB.AddComponent<Disabled>(binding.Value);
                                break;
                            case false when isCurrentlyDisabled:
                                ECB.RemoveComponent<Disabled>(binding.Value);
                                break;
                        }
                        break;

                    case ActivationTrack.PostPlaybackState.LeaveAsIs:
                        break;
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive), typeof(ActivationAnimatedComponent))]
        private partial struct ApplyRuntimeActivationJob : IJobEntity
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<bool>>.ReadOnly BlendData;
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;
            public EntityCommandBuffer.ParallelWriter ECB;

            private void Execute(Entity trackEntity, [EntityIndexInQuery] int sortKey, in TrackBinding binding)
            {
                bool shouldBeActive = BlendData.ContainsKey(binding.Value);
                bool isCurrentlyDisabled = DisabledLookup.HasComponent(binding.Value);
                switch (shouldBeActive)
                {
                    case true when isCurrentlyDisabled:
                        ECB.RemoveComponent<Disabled>(sortKey, binding.Value);
                        break;
                    case false when !isCurrentlyDisabled:
                        ECB.AddComponent<Disabled>(sortKey, binding.Value);
                        break;
                }
            }
        }
    }
}