using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Animations
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))][UpdateAfter(typeof(Rukhanka.Timeline.Systems.RukhankaTimelineTrackSystem))]
    public partial struct SimpleAnimatorSystem : ISystem
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

            new SimpleAnimatorJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                AnimDB = blobDB.animations
            }.ScheduleParallel();
        }

        [BurstCompile]
        private partial struct SimpleAnimatorJob : IJobEntity
        {
            public float DeltaTime;
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimDB;

            private void Execute(ref SimpleAnimatorComponent animator, ref DynamicBuffer<AnimationToProcessComponent> buffer)
            {
                animator.CurrentTime += DeltaTime * animator.CurrentSpeed;
                
                if (animator.IsTransitioning)
                {
                    animator.NextTime += DeltaTime * animator.NextSpeed;
                    animator.TransitionElapsed += DeltaTime;

                    if (animator.TransitionElapsed >= animator.TransitionDuration)
                    {
                        animator.CurrentClip = animator.NextClip;
                        animator.CurrentTime = animator.NextTime;
                        animator.CurrentSpeed = animator.NextSpeed;
                        animator.CurrentMotionId = animator.NextMotionId;
                        
                        animator.NextClip = default;
                        animator.TransitionDuration = 0f;
                        animator.TransitionElapsed = 0f;
                    }
                }

                buffer.Clear();

                float currentWeight = animator.IsTransitioning ? math.max(0f, 1f - (animator.TransitionElapsed / animator.TransitionDuration)) : 1f;
                AddClipToBuffer(ref buffer, animator.CurrentClip, animator.CurrentTime, currentWeight, animator.CurrentMotionId);

                if (animator.IsTransitioning && animator.NextClip.IsValid)
                {
                    float nextWeight = math.clamp(animator.TransitionElapsed / animator.TransitionDuration, 0f, 1f);
                    AddClipToBuffer(ref buffer, animator.NextClip, animator.NextTime, nextWeight, animator.NextMotionId);
                }
            }

            private void AddClipToBuffer(ref DynamicBuffer<AnimationToProcessComponent> buffer, Hash128 clipHash, float absoluteTime, float weight, uint motionId)
            {
                if (!clipHash.IsValid || weight <= 0f || !AnimDB.TryGetValue(clipHash, out var clipBlob))
                    return;

                float length = clipBlob.Value.length;
                float normalizedTime = 0f;

                if (length > 0f)
                {
                    normalizedTime = clipBlob.Value.looped 
                        ? math.frac(absoluteTime / length) 
                        : math.saturate(absoluteTime / length);
                }

                buffer.Add(new AnimationToProcessComponent
                {
                    animation = clipBlob,
                    time = normalizedTime,
                    weight = weight,
                    avatarMask = default,
                    blendMode = AnimationBlendingMode.Override,
                    layerIndex = 0,
                    layerWeight = 1f,
                    motionId = motionId
                });
            }
        }
    }
}