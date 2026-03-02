using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Rukhanka.Timeline.Systems
{[UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateAfter(typeof(BovineLabs.Timeline.Tracks.Animations.SimpleAnimatorSystem))]
    public partial struct RukhankaTimelineTrackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlobDatabaseSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var blobDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>();
            var simpleAnimatorLookup = SystemAPI.GetComponentLookup<SimpleAnimatorComponent>(false);
            var animationBufferLookup = SystemAPI.GetBufferLookup<AnimationToProcessComponent>(false);
            var clipWeightLookup = SystemAPI.GetComponentLookup<ClipWeight>(true);
            var localTimeLookup = SystemAPI.GetComponentLookup<LocalTime>(true);

            // 1. Evaluate Timeline Clips and overlay them natively into Rukhanka
            state.Dependency = new EvaluateTimelineClipsJob
            {
                AnimDB = blobDB.animations,
                AnimationBufferLookup = animationBufferLookup,
                ClipWeightLookup = clipWeightLookup,
                LocalTimeLookup = localTimeLookup
            }.Schedule(state.Dependency);

            // 2. Play the fallback Idle on the base Animator when the timeline finishes (if defined)
            state.Dependency = new StopTimelineTrackJob
            {
                SimpleAnimatorLookup = simpleAnimatorLookup
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct EvaluateTimelineClipsJob : IJobEntity
        {
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimDB;
            [ReadOnly] public ComponentLookup<ClipWeight> ClipWeightLookup;
            [ReadOnly] public ComponentLookup<LocalTime> LocalTimeLookup;
            
            // Non-parallel because multiple overlapping clips might write to the same entity buffer simultaneously
            [NativeDisableParallelForRestriction] 
            public BufferLookup<AnimationToProcessComponent> AnimationBufferLookup;

            private void Execute(Entity clipEntity, in RukhankaAnimationClipAnimated clipData, in TrackBinding binding)
            {
                if (!AnimationBufferLookup.HasBuffer(binding.Value)) return;
                if (!AnimDB.TryGetValue(clipData.AnimationHash, out var clipBlob)) return;

                var buffer = AnimationBufferLookup[binding.Value];
                
                // Get native timeline weights (supports ease-in, ease-out, crossfades naturally!)
                float weight = ClipWeightLookup.HasComponent(clipEntity) ? ClipWeightLookup[clipEntity].Value : 1f;
                float absoluteTime = (float)LocalTimeLookup[clipEntity].Value;

                if (weight > 0f)
                {
                    float length = clipBlob.Value.length;
                    float normalizedTime = 0f;

                    if (length > 0f)
                    {
                        normalizedTime = clipBlob.Value.looped 
                            ? math.frac(absoluteTime / length) 
                            : math.saturate(absoluteTime / length);
                    }

                    // Append directly to the buffer as Layer 1
                    buffer.Add(new AnimationToProcessComponent
                    {
                        animation = clipBlob,
                        time = normalizedTime,
                        weight = weight,
                        avatarMask = default,
                        blendMode = AnimationBlendingMode.Override,
                        layerIndex = 1,  // Layer 1 allows Timeline to flawlessly take over Layer 0 (Simple Animator)
                        layerWeight = 1f,
                        motionId = (uint)clipEntity.Index // Unique tracking motion ID so Rukhanka respects it 
                    });
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(TimelineActive))][WithAll(typeof(TimelineActivePrevious))]
        private partial struct StopTimelineTrackJob : IJobEntity
        {
            public ComponentLookup<SimpleAnimatorComponent> SimpleAnimatorLookup;

            private void Execute(in RukhankaTimelineTrack trackData, in TrackBinding binding)
            {
                if (trackData.ExitIdleClipHash.IsValid && SimpleAnimatorLookup.HasComponent(binding.Value))
                {
                    var animator = SimpleAnimatorLookup[binding.Value];

                    // Fire and forget returning to a specific base state
                    animator.Play(trackData.ExitIdleClipHash, 1f, trackData.ExitTransitionDuration, forceRestart: true);

                    SimpleAnimatorLookup[binding.Value] = animator;
                }
            }
        }
    }
}