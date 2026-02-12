using BovineLabs.Core.Jobs;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.GameObjects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct ActivationTrackSystem : ISystem
    {
        private TrackBlendImpl<float, ActivationAnimatedComponent> impl;

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
            var disabledLookup = SystemAPI.GetComponentLookup<Disabled>(true);

            new ActivateResetJob
            {
                DisabledLookup = disabledLookup
            }.ScheduleParallel();

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            new DeactivateResetJob
            {
                DisabledLookup = disabledLookup,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
            }.Schedule();

            var blendData = this.impl.Update(ref state);

            state.Dependency = new ApplyActivationJob
            {
                BlendData = blendData,
                DisabledLookup = disabledLookup,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        [WithNone(typeof(TimelineActivePrevious))]
        private partial struct ActivateResetJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;

            private void Execute(ref ActivationResetOnDeactivate reset, in TrackBinding binding)
            {
                reset.WasDisabled = this.DisabledLookup.HasComponent(binding.Value);
            }
        }

        [BurstCompile]
        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious))]
        private partial struct DeactivateResetJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;
            public EntityCommandBuffer ECB;

            private void Execute(in ActivationResetOnDeactivate reset, in TrackBinding binding)
            {
                var currentlyDisabled = this.DisabledLookup.HasComponent(binding.Value);

                if (reset.WasDisabled && !currentlyDisabled)
                {
                    this.ECB.AddComponent<Disabled>(binding.Value);
                }
                else if (!reset.WasDisabled && currentlyDisabled)
                {
                    this.ECB.RemoveComponent<Disabled>(binding.Value);
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive), typeof(ActivationTrackComponent))]
        private partial struct ApplyActivationJob : IJobEntity
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<float>>.ReadOnly BlendData;
            [ReadOnly] public ComponentLookup<Disabled> DisabledLookup;
            public EntityCommandBuffer.ParallelWriter ECB;

            private void Execute(Entity entity, [EntityIndexInQuery] int sortKey, in TrackBinding binding)
            {
                bool shouldBeActive = this.BlendData.ContainsKey(binding.Value);
                bool isDisabled = this.DisabledLookup.HasComponent(binding.Value);

                if (shouldBeActive && isDisabled)
                {
                    this.ECB.RemoveComponent<Disabled>(sortKey, binding.Value);
                }
                else if (!shouldBeActive && !isDisabled)
                {
                    this.ECB.AddComponent<Disabled>(sortKey, binding.Value);
                }
            }
        }
    }
}