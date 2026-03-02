using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Unity.Burst;
using Unity.Entities;

namespace Rukhanka.Timeline.Systems
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateBefore(typeof(BovineLabs.Timeline.Tracks.Animations.SimpleAnimatorSystem))]
    public partial struct RukhankaTimelineTrackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimpleAnimatorComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simpleAnimatorLookup = SystemAPI.GetComponentLookup<SimpleAnimatorComponent>(false);

            state.Dependency = new StartTimelineClipJob
            {
                SimpleAnimatorLookup = simpleAnimatorLookup
            }.Schedule(state.Dependency);

            state.Dependency = new SyncTimelineTimeJob
            {
                SimpleAnimatorLookup = simpleAnimatorLookup
            }.Schedule(state.Dependency);

            state.Dependency = new StopTimelineTrackJob
            {
                SimpleAnimatorLookup = simpleAnimatorLookup
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct StartTimelineClipJob : IJobEntity
        {
            public ComponentLookup<SimpleAnimatorComponent> SimpleAnimatorLookup;

            private void Execute(in RukhankaAnimationClipAnimated clipData, in TrackBinding binding)
            {
                if (SimpleAnimatorLookup.HasComponent(binding.Value))
                {
                    var animator = SimpleAnimatorLookup[binding.Value];

                    animator.Play(clipData.AnimationHash, forceRestart: true);

                    SimpleAnimatorLookup[binding.Value] = animator;
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct SyncTimelineTimeJob : IJobEntity
        {
            public ComponentLookup<SimpleAnimatorComponent> SimpleAnimatorLookup;

            private void Execute(in RukhankaAnimationClipAnimated clipData, in LocalTime localTime,
                in TrackBinding binding)
            {
                if (SimpleAnimatorLookup.HasComponent(binding.Value))
                {
                    var animator = SimpleAnimatorLookup[binding.Value];
                    bool changed = false;

                    if (animator.CurrentClip == clipData.AnimationHash)
                    {
                        animator.CurrentTime = (float)localTime.Value;
                        changed = true;
                    }

                    if (animator.NextClip == clipData.AnimationHash)
                    {
                        animator.NextTime = (float)localTime.Value;
                        changed = true;
                    }

                    if (changed)
                    {
                        SimpleAnimatorLookup[binding.Value] = animator;
                    }
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(TimelineActive))]
        [WithAll(typeof(TimelineActivePrevious))]
        private partial struct StopTimelineTrackJob : IJobEntity
        {
            public ComponentLookup<SimpleAnimatorComponent> SimpleAnimatorLookup;

            private void Execute(in RukhankaTimelineTrack trackData, in TrackBinding binding)
            {
                if (trackData.ExitIdleClipHash.IsValid && SimpleAnimatorLookup.HasComponent(binding.Value))
                {
                    var animator = SimpleAnimatorLookup[binding.Value];

                    animator.Play(trackData.ExitIdleClipHash, 1f, trackData.ExitTransitionDuration, forceRestart: true);

                    SimpleAnimatorLookup[binding.Value] = animator;
                }
            }
        }
    }
}