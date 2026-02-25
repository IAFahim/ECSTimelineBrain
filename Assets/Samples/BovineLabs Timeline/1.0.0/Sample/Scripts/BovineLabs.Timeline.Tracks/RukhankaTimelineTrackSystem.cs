using BovineLabs.Core;
using BovineLabs.Core.Jobs;
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            new RukhankaTimelineTrackSystemJob()
            {
                animDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>().animations,
                AnimationToProcessComponentBufferLookup = SystemAPI.GetBufferLookup<AnimationToProcessComponent>(),
                AnimationTime = (float)SystemAPI.Time.ElapsedTime,
            }.ScheduleParallel();

            var blendData = impl.Update(ref state);
            var blLogger = SystemAPI.GetSingleton<BLLogger>();
            new WriteRukhankaTimelineJob()
            {
                BlendData = blendData,
                BlLogger = blLogger,
            }.ScheduleParallel(blendData, 64, state.Dependency);
        }

        [BurstCompile]
        public partial struct RukhankaTimelineTrackSystemJob : IJobEntity
        {
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> animDB;
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<AnimationToProcessComponent> AnimationToProcessComponentBufferLookup;
            public float AnimationTime;

            void Execute(ref RukhankaAnimationClipAnimated rukhankaAnimationClipAnimated, in TrackBinding trackBinding)
            {
                var animationToProcessComponents = AnimationToProcessComponentBufferLookup[trackBinding.Value];
                ScriptedAnimator.ResetAnimationState(ref animationToProcessComponents);
                animDB.TryGetValue(rukhankaAnimationClipAnimated.AnimationHash, out var clip0Blob);
                ScriptedAnimator.PlayAnimation(ref animationToProcessComponents, clip0Blob, AnimationTime);
            }
        }
        
        [BurstCompile]
        private struct WriteRukhankaTimelineJob : IJobParallelHashMapDefer
        {
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> animDB;
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<AnimationToProcessComponent> AnimationToProcessComponentBufferLookup;
            public float AnimationTime;
            
            
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<float>>.ReadOnly BlendData;
            [WriteOnly] public BLLogger BlLogger;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(BlendData, entryIndex, out var entity, out var target);
                BlLogger.LogDebug512(target.ToString());
                
            }
        }
    }
}