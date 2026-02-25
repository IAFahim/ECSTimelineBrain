using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Rukhanka.Timeline.Systems
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateBefore(typeof(AnimationProcessSystem))]
    public partial struct RukhankaTimelineTrackSystem : ISystem
    {
        private TrackBlendImpl<float, RukhankaAnimationClipAnimated> impl;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            impl.OnCreate(ref state);
            state.RequireForUpdate<BlobDatabaseSingleton>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            impl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var blendData = impl.Update(ref state);
            new RukhankaTimelineTrackSystemJob()
            {
                animDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>().animations,
                AnimationToProcessComponentBufferLookup = SystemAPI.GetBufferLookup<AnimationToProcessComponent>(),
                AnimationTime = (float)SystemAPI.Time.ElapsedTime
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct RukhankaTimelineTrackSystemJob : IJobEntity
        {
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> animDB;
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<AnimationToProcessComponent> AnimationToProcessComponentBufferLookup;
            public float AnimationTime;

            void Execute(ref RukhankaAnimationClipAnimated rukhankaAnimationClipAnimated,in TrackBinding trackBinding)
            {
                var animationToProcessComponents = AnimationToProcessComponentBufferLookup[trackBinding.Value];
                ScriptedAnimator.ResetAnimationState(ref animationToProcessComponents);
                animDB.TryGetValue(rukhankaAnimationClipAnimated.AnimationHash, out var clip0Blob);
                ScriptedAnimator.PlayAnimation(ref animationToProcessComponents, clip0Blob, AnimationTime);
            }
        }
    }
}